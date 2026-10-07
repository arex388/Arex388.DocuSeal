using Arex388.DocuSeal.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Benchmarks;

/// <summary>
/// Builds IDocuSealClient instances backed by the shared fixture handler. The
/// named client's base address is unroutable so a wiring regression that drops
/// the mock handler fails fast instead of hitting the live API.
/// </summary>
internal static class BenchmarkServiceProvider {
	private const string AuthorizationToken = "benchmark-token";
	private static readonly Uri _baseAddress = new("https://localhost:9/");

	public static IDocuSealClient CreateClient() {
		var services = new ServiceCollection();

		services.AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = AuthorizationToken
		}).AddHttpClient(nameof(IDocuSealClient), hc => hc.BaseAddress = _baseAddress)
				.ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler());

		return services.BuildServiceProvider().GetRequiredService<IDocuSealClient>();
	}
}
