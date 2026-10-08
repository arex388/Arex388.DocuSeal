using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Tests.Unit;

public sealed class DocuSealClientFactoryTest {
	private readonly ITestOutputHelper _console;
	private readonly IDocuSealClientFactory _docuSealFactory;

	public DocuSealClientFactoryTest(
		ITestOutputHelper console) {
		var services = new ServiceCollection().AddDocuSeal().BuildServiceProvider();

		_console = console;
		_docuSealFactory = services.GetRequiredService<IDocuSealClientFactory>();
	}

	[Fact]
	public void CreateAndCacheClient() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	========================================================================
		//	Act
		//	========================================================================

		var created = _docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "factory-test-token"
		});
		var cached = _docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "factory-test-token"
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		_console.WriteLineWithHeader(nameof(created), created);
		_console.WriteLineWithHeader(nameof(cached), cached);

		created.Should().BeSameAs(cached);
	}

	[Fact]
	public void CreateClients() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	========================================================================
		//	Act
		//	========================================================================

		var client1 = _docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "factory-test-token"
		});
		var client2 = _docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = string.Empty
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		_console.WriteLineWithHeader(nameof(client1), client1);
		_console.WriteLineWithHeader(nameof(client2), client2);

		client1.Should().NotBeSameAs(client2);
	}

	[Fact]
	public async Task CreatedClient_SendsToGlobalHost_WithOneAuthorizationToken() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		const string authorizationToken = "factory-header-token";
		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		services.AddDocuSeal()
				.AddHttpClient(nameof(IDocuSealClient))
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSealFactory = services.BuildServiceProvider().GetRequiredService<IDocuSealClientFactory>();

		//	========================================================================
		//	Act
		//	========================================================================

		var docuSeal = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = authorizationToken
		});

		await docuSeal.ListTemplatesAsync();

		//	========================================================================
		//	Assert
		//	========================================================================

		//	CapturingHandler reads the header with Single(), so a duplicated
		//	X-Auth-Token throws inside the handler and no request is captured.
		var request = handler.Requests.Should().ContainSingle().Subject;

		request.Uri.AbsoluteUri.Should().StartWith("https://api.docuseal.com/templates");
		request.AuthorizationToken.Should().Be(authorizationToken);
	}

	[Fact]
	public async Task CreatedClient_AfterSingleAccountRegistration_SendsOnlyTheFactoryToken() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		const string authorizationToken = "factory-header-token";
		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		//	The single-account registration configures the shared named client
		//	with its own token; the factory must replace it, not append to it.
		services.AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = "single-account-token"
		}).AddDocuSeal()
				.AddHttpClient(nameof(IDocuSealClient))
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSealFactory = services.BuildServiceProvider().GetRequiredService<IDocuSealClientFactory>();

		//	========================================================================
		//	Act
		//	========================================================================

		var docuSeal = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = authorizationToken
		});

		await docuSeal.ListTemplatesAsync();

		//	========================================================================
		//	Assert
		//	========================================================================

		handler.Requests.Should().ContainSingle().Which.AuthorizationToken.Should().Be(authorizationToken);
	}

	[Theory]
	[InlineData(DocuSealRegion.Global, "https://api.docuseal.com/templates")]
	[InlineData(DocuSealRegion.Eu, "https://api.docuseal.eu/templates")]
	public async Task CreatedClient_SendsToTheRegionHost(
		DocuSealRegion region,
		string expectedUri) {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		services.AddDocuSeal()
				.AddHttpClient(nameof(IDocuSealClient))
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSealFactory = services.BuildServiceProvider().GetRequiredService<IDocuSealClientFactory>();

		//	========================================================================
		//	Act
		//	========================================================================

		var docuSeal = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "region-token",
			Region = region
		});

		await docuSeal.ListTemplatesAsync();

		//	========================================================================
		//	Assert
		//	========================================================================

		var request = handler.Requests.Should().ContainSingle().Subject;

		request.Uri.AbsoluteUri.Should().StartWith(expectedUri);
		request.AuthorizationToken.Should().Be("region-token");
	}

	[Fact]
	public async Task CreatedClients_SameTokenInTwoRegions_AreSeparateClients_EachSendingToItsOwnHost() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var handler = new CapturingHandler("{}");
		var services = new ServiceCollection();

		services.AddDocuSeal()
				.AddHttpClient(nameof(IDocuSealClient))
				.ConfigurePrimaryHttpMessageHandler(() => handler);

		var docuSealFactory = services.BuildServiceProvider().GetRequiredService<IDocuSealClientFactory>();

		//	========================================================================
		//	Act
		//	========================================================================

		var global = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "shared-token"
		});
		var eu = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "shared-token",
			Region = DocuSealRegion.Eu
		});
		var globalCached = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "shared-token",
			Region = DocuSealRegion.Global
		});
		var euCached = docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = "shared-token",
			Region = DocuSealRegion.Eu
		});

		await global.ListTemplatesAsync();
		await eu.ListTemplatesAsync();

		//	========================================================================
		//	Assert
		//	========================================================================

		global.Should().NotBeSameAs(eu);
		globalCached.Should().BeSameAs(global);
		euCached.Should().BeSameAs(eu);

		handler.Requests.Select(r => r.Uri.Host).Should().Equal("api.docuseal.com", "api.docuseal.eu");
	}
}