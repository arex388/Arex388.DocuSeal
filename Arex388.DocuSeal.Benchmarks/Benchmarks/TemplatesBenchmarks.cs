using BenchmarkDotNet.Attributes;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class TemplatesBenchmarks {
	private static readonly CreateTemplateFromHtml.Request _createFromHtmlRequest = new() {
		Html = "<p>Test Document</p>",
		Name = "Test Template"
	};

	//	Every string parameter set, each with characters that need escaping, so the query-encoding path is measured.
	private static readonly ListTemplates.Request _listWithParametersRequest = new() {
		ExternalId = "external id/1",
		Folder = "Sales & Contracts",
		Search = "non-disclosure agreement",
		Slug = "slug+value"
	};

	private readonly IDocuSealClient _docuSeal = BenchmarkServiceProvider.CreateClient();

	[Benchmark]
	public Task<CreateTemplateFromHtml.Response> CreateFromHtml() => _docuSeal.CreateTemplateFromHtmlAsync(_createFromHtmlRequest);

	[Benchmark]
	public Task<ListTemplates.Response> List() => _docuSeal.ListTemplatesAsync();

	[Benchmark]
	public Task<ListTemplates.Response> ListWithParameters() => _docuSeal.ListTemplatesAsync(_listWithParametersRequest);
}
