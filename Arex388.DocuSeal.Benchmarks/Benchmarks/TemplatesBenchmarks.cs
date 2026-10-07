using BenchmarkDotNet.Attributes;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class TemplatesBenchmarks {
	private readonly IDocuSealClient _docuSeal = BenchmarkServiceProvider.CreateClient();

	[Benchmark]
	public Task<ListTemplates.Response> List() => _docuSeal.ListTemplatesAsync();
}
