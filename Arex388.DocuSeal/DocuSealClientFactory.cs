using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal;

internal sealed class DocuSealClientFactory(
	IServiceProvider services,
	IMemoryCache cache) :
	IDocuSealClientFactory {
	private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new() {
		SlidingExpiration = TimeSpan.MaxValue
	};

	private readonly IServiceProvider _services = services;
	private readonly IMemoryCache _cache = cache;

	/// <inheritdoc />
	public IDocuSealClient CreateClient(
		DocuSealClientOptions options) {
		var key = $"{nameof(Arex388)}.{nameof(DocuSeal)}.Key[{options.AuthorizationToken}]";

		if (_cache.TryGetValue(key, out IDocuSealClient? docuSealClient)
			&& docuSealClient is not null) {
			return docuSealClient;
		}

		var httpClientFactory = _services.GetRequiredService<IHttpClientFactory>();
		var httpClient = httpClientFactory.CreateClient(nameof(IDocuSealClient));

		httpClient.BaseAddress = HttpClientHelper.BaseAddress;
		httpClient.DefaultRequestHeaders.Add("X-Auth-Token", options.AuthorizationToken);

		docuSealClient = new DocuSealClient(_services, httpClient);

		_cache.Set(key, docuSealClient, _cacheEntryOptions);

		return docuSealClient;
	}
}