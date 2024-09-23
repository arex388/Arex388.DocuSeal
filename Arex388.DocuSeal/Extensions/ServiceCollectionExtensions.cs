using Arex388.DocuSeal;
using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <c>IServiceCollection</c> extensions.
/// </summary>
public static class ServiceCollectionExtensions {
	/// <summary>
	/// Add the DocuSeal.co API client factory for interacting with multiple accounts.
	/// </summary>
	/// <param name="services">The services collection.</param>
	/// <returns>The services collection.</returns>
	public static IServiceCollection AddDocuSeal(
		this IServiceCollection services) => services.AddHttpClient()
													 .AddMemoryCache()
													 .AddValidatorsFromAssemblyContaining<IDocuSealClient>(includeInternalTypes: true, lifetime: ServiceLifetime.Singleton)
													 .AddSingleton<IDocuSealClientFactory, DocuSealClientFactory>();

	/// <summary>
	/// Add the DocuSeal.co client for interacting with a single account.
	/// </summary>
	/// <param name="services">The services collection.</param>
	/// <param name="options">The client's configuration options.</param>
	/// <returns>The services collection.</returns>
	public static IServiceCollection AddDocuSeal(
		this IServiceCollection services,
		DocuSealClientOptions options) {
		services.AddHttpClient<IDocuSealClient>(
			hc => {
				hc.BaseAddress = HttpClientHelper.BaseAddress;
				hc.DefaultRequestHeaders.Add("X-Auth-Token", options.AuthorizationToken);
			});

		return services.AddValidatorsFromAssemblyContaining<IDocuSealClient>(includeInternalTypes: true, lifetime: ServiceLifetime.Singleton)
					   .AddSingleton<IDocuSealClient>(
						   sp => new DocuSealClient(sp));
	}
}