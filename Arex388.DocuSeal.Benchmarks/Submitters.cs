using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class Submitters {
	private readonly IDocuSealClient _docuSeal;

	public Submitters() {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken
		}).BuildServiceProvider();

		_docuSeal = services.GetRequiredService<IDocuSealClient>();
	}

	[Benchmark]
	public Task List() => _docuSeal.ListSubmittersAsync();
}