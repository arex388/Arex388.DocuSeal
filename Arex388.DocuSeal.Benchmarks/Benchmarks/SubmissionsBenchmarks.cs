using BenchmarkDotNet.Attributes;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class SubmissionsBenchmarks {
	private readonly IDocuSealClient _docuSeal = BenchmarkServiceProvider.CreateClient();

	[Benchmark]
	public Task<ListSubmissions.Response> List() => _docuSeal.ListSubmissionsAsync();
}
