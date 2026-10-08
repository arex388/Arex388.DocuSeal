using Arex388.DocuSeal.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Builds IDocuSealClient instances backed by mock HTTP handlers. The named
/// client's base address is unroutable so a wiring regression that drops the
/// mock handler fails fast on connection refused instead of hitting the live API.
/// </summary>
internal static class TestClients {
	public const string AuthorizationToken = "unit-test-token";
	public static readonly Uri BaseAddress = new("https://localhost:9/");

	/// <summary>
	/// The serializer options the client actually uses, read by reflection from
	/// <c>DocuSealClient</c>'s static field. The converters are internal and only
	/// take effect through this registration, so tests that bind or write JSON
	/// run against it rather than against hand-built options.
	/// </summary>
	public static readonly JsonSerializerOptions JsonOptions = (JsonSerializerOptions)typeof(IDocuSealClient).Assembly
		.GetType("Arex388.DocuSeal.DocuSealClient", throwOnError: true)!
		.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
		.Single(f => f.FieldType == typeof(JsonSerializerOptions))
		.GetValue(null)!;

	public static IDocuSealClient Create(
		HttpMessageHandler handler) {
		var services = new ServiceCollection();

		services.AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = AuthorizationToken
		}).AddHttpClient(nameof(IDocuSealClient), hc => hc.BaseAddress = BaseAddress)
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		return services.BuildServiceProvider().GetRequiredService<IDocuSealClient>();
	}

	/// <summary>
	/// Serves the shared TestData/Responses fixtures by route and method.
	/// </summary>
	public static IDocuSealClient CreateWithFixtures() => Create(new MockHttpMessageHandler());

	/// <summary>
	/// Serves the same JSON body for every request, recording each request.
	/// </summary>
	public static IDocuSealClient CreateWithJson(
		string json,
		out CapturingHandler handler,
		HttpStatusCode statusCode = HttpStatusCode.OK) {
		handler = new CapturingHandler(json, statusCode);

		return Create(handler);
	}
}

/// <summary>
/// A request the <see cref="CapturingHandler"/> saw, with its body read eagerly.
/// </summary>
internal sealed record CapturedRequest(
	HttpMethod Method,
	Uri Uri,
	string? AuthorizationToken,
	string? Body);

internal sealed class CapturingHandler(
	string json,
	HttpStatusCode statusCode = HttpStatusCode.OK) :
	HttpMessageHandler {
	public IList<CapturedRequest> Requests { get; } = [];

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken) {
		var body = request.Content is null
			? null
			: await request.Content.ReadAsStringAsync(cancellationToken);
		var authorizationToken = request.Headers.TryGetValues("X-Auth-Token", out var values)
			? values.Single()
			: null;

		Requests.Add(new CapturedRequest(request.Method, request.RequestUri!, authorizationToken, body));

		return new HttpResponseMessage(statusCode) {
			Content = new StringContent(json, Encoding.UTF8, "application/json"),
			RequestMessage = request
		};
	}
}

/// <summary>
/// Throws on every request, the way a dropped connection or DNS failure does.
/// </summary>
internal sealed class ThrowingHandler :
	HttpMessageHandler {
	public int Calls { get; private set; }

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken) {
		Calls++;

		throw new HttpRequestException("Simulated transport failure.");
	}
}
