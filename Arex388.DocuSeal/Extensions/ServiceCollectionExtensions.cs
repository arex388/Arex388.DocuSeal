using Arex388.DocuSeal;
using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class ServiceCollectionExtensions {
	/// <param name="services">The services collection.</param>
	extension(
		IServiceCollection services) {
		/// <summary>
		/// Add the DocuSeal.co API client factory for interacting with multiple accounts.
		/// </summary>
		/// <returns>The services collection.</returns>
		public IServiceCollection AddDocuSeal() {
			AddDocuSealHttpClient(services);

			return services.AddMemoryCache()
						   .AddValidatorsFromAssemblyContaining<IDocuSealClient>(includeInternalTypes: true, lifetime: ServiceLifetime.Singleton)
						   .AddSingleton<IDocuSealClientFactory, DocuSealClientFactory>();
		}

		/// <summary>
		/// Add the DocuSeal.co API client for interacting with a single account.
		/// </summary>
		/// <param name="options">The client's configuration options.</param>
		/// <returns>The services collection.</returns>
		public IServiceCollection AddDocuSeal(
			DocuSealClientOptions options) {
			AddDocuSealHttpClient(services, options);

			return services.AddValidatorsFromAssemblyContaining<IDocuSealClient>(includeInternalTypes: true, lifetime: ServiceLifetime.Singleton)
						   .AddSingleton(options)
						   .AddSingleton<IDocuSealClient>(
							   sp => new DocuSealClient(sp));
		}
	}

	//	============================================================================
	//	Utilities
	//	============================================================================

	private static void AddDocuSealHttpClient(
		IServiceCollection services,
		DocuSealClientOptions? options = null) => services.AddHttpClient(nameof(IDocuSealClient), hc => {
			hc.BaseAddress = HttpClientHelper.BaseAddress;

			if (options is not null) {
				hc.DefaultRequestHeaders.Add("X-Auth-Token", options.AuthorizationToken);
			}
		});
}