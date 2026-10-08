using BenchmarkDotNet.Attributes;
using System.Text;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class WebhooksBenchmarks {
	private static readonly string _specDirectory = Path.Combine(AppContext.BaseDirectory, "Spec");

	private readonly string _formCompleted = Spec("form-completed");
	private readonly byte[] _formCompletedUtf8;
	private readonly string _templateCreated = Spec("template-created");
	private readonly byte[] _templateCreatedUtf8;

	public WebhooksBenchmarks() {
		_formCompletedUtf8 = Encoding.UTF8.GetBytes(_formCompleted);
		_templateCreatedUtf8 = Encoding.UTF8.GetBytes(_templateCreated);
	}

	[Benchmark]
	public WebhookEvent? ParseFormCompletedString() => DocuSealWebhook.Parse(_formCompleted);

	[Benchmark]
	public WebhookEvent? ParseFormCompletedUtf8() => DocuSealWebhook.Parse(_formCompletedUtf8);

	[Benchmark]
	public WebhookEvent? ParseTemplateCreatedString() => DocuSealWebhook.Parse(_templateCreated);

	[Benchmark]
	public WebhookEvent? ParseTemplateCreatedUtf8() => DocuSealWebhook.Parse(_templateCreatedUtf8);

	private static string Spec(
		string name) => File.ReadAllText(Path.Combine(_specDirectory, $"webhook-{name}.json"));
}
