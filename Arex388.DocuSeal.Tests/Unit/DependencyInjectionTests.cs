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
	public void RequestTypes_AreDiscovered() => RequestTypes.Should().HaveCount(15);

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
