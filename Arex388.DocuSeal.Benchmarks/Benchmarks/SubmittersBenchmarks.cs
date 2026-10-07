using BenchmarkDotNet.Attributes;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class SubmittersBenchmarks {
	private readonly IDocuSealClient _docuSeal = BenchmarkServiceProvider.CreateClient();

	[Benchmark]
	public Task<ListSubmitters.Response> List() => _docuSeal.ListSubmittersAsync();
}
