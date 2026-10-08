using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal;

internal sealed class DocuSealClientFactory(
	IServiceProvider services,
	IMemoryCache cache) :
	IDocuSealClientFactory {
	private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new() {
		AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
		SlidingExpiration = TimeSpan.FromHours(1)
	};

	private readonly IServiceProvider _services = services;
	private readonly IMemoryCache _cache = cache;
	private readonly object _missLock = new();

	public IDocuSealClient CreateClient(
		DocuSealClientOptions options) {
		//	Resolved up front so an undefined region throws here, not inside the
		//	Lazy, where the cache would keep the faulted entry.
		var baseAddress = HttpClientHelper.GetBaseAddress(options.Region);
		var key = $"{nameof(Arex388)}.{nameof(DocuSeal)}.Key[{options.AuthorizationToken}|{options.Region}]";

		//	Fast path: a cache hit skips the lock entirely.
		if (_cache.TryGetValue(key, out Lazy<IDocuSealClient>? cached)
			&& cached is not null) {
			return cached.Value;
		}

		//	The cache's value factory is not synchronized, so two racing first
		//	calls for one token could each build a client. The lock serializes
		//	entry creation (once per token and region per cache lifetime); the Lazy keeps
		//	client construction outside the lock.
		Lazy<IDocuSealClient>? lazy;

		lock (_missLock) {
			if (!_cache.TryGetValue(key, out lazy)
				|| lazy is null) {
				lazy = new Lazy<IDocuSealClient>(() => {
					var httpClientFactory = _services.GetRequiredService<IHttpClientFactory>();
					var httpClient = httpClientFactory.CreateClient(nameof(IDocuSealClient));

					httpClient.BaseAddress = baseAddress;
					httpClient.SetAuthorizationToken(options.AuthorizationToken);

					return new DocuSealClient(_services, httpClient);
				}, LazyThreadSafetyMode.ExecutionAndPublication);

				_cache.Set(key, lazy, _cacheEntryOptions);
			}
		}

		return lazy.Value;
	}
}