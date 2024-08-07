using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class Templates {
	private readonly IDocuSealClient _docuSeal;

	public Templates() {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken
		}).BuildServiceProvider();

		_docuSeal = services.GetRequiredService<IDocuSealClient>();
	}

	[Benchmark]
	public Task List() => _docuSeal.ListTemplatesAsync();
}