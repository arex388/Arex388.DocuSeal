using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Tests;

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
			AuthorizationToken = Config.AuthorizationToken1
		});
		var cached = _docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken1
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
			AuthorizationToken = Config.AuthorizationToken1
		});
		var client2 = _docuSealFactory.CreateClient(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken2
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		_console.WriteLineWithHeader(nameof(client1), client1);
		_console.WriteLineWithHeader(nameof(client2), client2);

		client1.Should().NotBeSameAs(client2);
	}
}