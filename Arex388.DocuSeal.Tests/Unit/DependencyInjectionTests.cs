using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Tests.Unit;

public sealed class DependencyInjectionTests {
	/// <summary>
	/// Every public slice's <c>Request</c> type, discovered from the library so a
	/// new slice is covered without editing this list.
	/// </summary>
	public static TheoryData<Type> RequestTypes => [
		.. typeof(IDocuSealClient).Assembly.GetExportedTypes()
								   .Where(t => t is {
									   IsNested: true,
									   Name: "Request"
								   })
								   .OrderBy(t => t.FullName, StringComparer.Ordinal)
	];

	[Fact]
	public void AddDocuSeal_WithoutOptions_ResolvesFactory() {
		var services = new ServiceCollection().AddDocuSeal().BuildServiceProvider();

		services.GetRequiredService<IDocuSealClientFactory>().Should().NotBeNull();
	}

	[Fact]
	public void AddDocuSeal_WithOptions_ResolvesClient() {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = "unit-test-token"
		}).BuildServiceProvider();

		var client = services.GetRequiredService<IDocuSealClient>();

		client.Should().NotBeNull();
		services.GetRequiredService<IDocuSealClient>().Should().BeSameAs(client, "the single-account client is a singleton");
	}

	[Fact]
	public async Task AddDocuSealWithOptions_Twice_SendsOnlyTheSecondToken() {
		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		services.AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = "first-token"
		}).AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = "second-token"
		}).AddHttpClient(nameof(IDocuSealClient), hc => hc.BaseAddress = TestClients.BaseAddress)
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSeal = services.BuildServiceProvider().GetRequiredService<IDocuSealClient>();

		await docuSeal.ListTemplatesAsync();

		//	CapturingHandler reads the header with Single(), so a duplicated
		//	X-Auth-Token throws inside the handler and no request is captured.
		handler.Requests.Should().ContainSingle().Which.AuthorizationToken.Should().Be("second-token");
	}

	[Fact]
	public void Options_DefaultRegion_IsGlobal() => new DocuSealClientOptions {
		AuthorizationToken = "unit-test-token"
	}.Region.Should().Be(DocuSealRegion.Global);

	[Theory]
	[InlineData(null, "https://api.docuseal.com/templates")]
	[InlineData(DocuSealRegion.Global, "https://api.docuseal.com/templates")]
	[InlineData(DocuSealRegion.Eu, "https://api.docuseal.eu/templates")]
	public async Task AddDocuSealWithOptions_SendsToTheRegionHost(
		DocuSealRegion? region,
		string expectedUri) {
		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		services.AddDocuSeal(region is null
				? new DocuSealClientOptions {
					AuthorizationToken = "unit-test-token"
				}
				: new DocuSealClientOptions {
					AuthorizationToken = "unit-test-token",
					Region = region.Value
				})
				.AddHttpClient(nameof(IDocuSealClient))
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSeal = services.BuildServiceProvider().GetRequiredService<IDocuSealClient>();

		await docuSeal.ListTemplatesAsync();

		handler.Requests.Should().ContainSingle().Which.Uri.AbsoluteUri.Should().StartWith(expectedUri);
	}

	[Fact]
	public async Task AddDocuSealWithEuOptions_ThenFactoryRegistration_StillSendsToTheEuHost() {
		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		services.AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = "unit-test-token",
			Region = DocuSealRegion.Eu
		}).AddDocuSeal()
				.AddHttpClient(nameof(IDocuSealClient))
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSeal = services.BuildServiceProvider().GetRequiredService<IDocuSealClient>();

		await docuSeal.ListTemplatesAsync();

		handler.Requests.Should().ContainSingle().Which.Uri.AbsoluteUri.Should().StartWith("https://api.docuseal.eu/templates");
	}

	[Fact]
	public void RequestTypes_AreDiscovered() => RequestTypes.Should().HaveCount(19);

	[Theory]
	[MemberData(nameof(RequestTypes))]
	public void AddDocuSeal_ResolvesValidator_ForEveryRequestType(
		Type requestType) {
		var services = new ServiceCollection().AddDocuSeal().BuildServiceProvider();

		var validator = services.GetService(typeof(IValidator<>).MakeGenericType(requestType));

		validator.Should().NotBeNull($"{requestType.FullName} must have a registered validator");
	}

	[Theory]
	[MemberData(nameof(RequestTypes))]
	public void AddDocuSealWithOptions_ResolvesValidator_ForEveryRequestType(
		Type requestType) {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = "unit-test-token"
		}).BuildServiceProvider();

		var validator = services.GetService(typeof(IValidator<>).MakeGenericType(requestType));

		validator.Should().NotBeNull($"{requestType.FullName} must have a registered validator");
	}
}
