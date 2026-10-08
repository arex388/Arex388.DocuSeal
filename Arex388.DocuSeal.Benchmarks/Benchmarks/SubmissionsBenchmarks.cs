using BenchmarkDotNet.Attributes;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class SubmissionsBenchmarks {
	private const string FileBase64 = "JVBERi0xLjQK";

	private static readonly CreateSubmission.Request _createRequest = new() {
		Submitters = [
			new CreateSubmission.RequestSubmitter {
				Email = "signer1@example.com",
				Role = "First Party"
			},
			new CreateSubmission.RequestSubmitter {
				Email = "signer2@example.com",
				Role = "Second Party"
			}
		],
		TemplateId = new TemplateId(1001)
	};
	private static readonly CreateSubmissionFromPdf.Request _createFromPdfRequest = new() {
		Documents = [
			new CreateSubmissionFromPdf.RequestDocument {
				FileBase64 = FileBase64,
				Name = "Test Document"
			}
		],
		Submitters = [
			new CreateSubmission.RequestSubmitter {
				Email = "signer1@example.com"
			}
		]
	};
	private static readonly UpdateSubmission.Request _updateClearExpirationRequest = new() {
		ClearExpiration = true,
		Id = new SubmissionId(2001),
		Name = "Renamed Submission"
	};

	private readonly IDocuSealClient _docuSeal = BenchmarkServiceProvider.CreateClient();

	[Benchmark]
	public Task<CreateSubmission.Response> Create() => _docuSeal.CreateSubmissionAsync(_createRequest);

	[Benchmark]
	public Task<CreateSubmissionFromPdf.Response> CreateFromPdf() => _docuSeal.CreateSubmissionFromPdfAsync(_createFromPdfRequest);

	[Benchmark]
	public Task<ListSubmissions.Response> List() => _docuSeal.ListSubmissionsAsync();

	[Benchmark]
	public Task<UpdateSubmission.Response> UpdateClearExpiration() => _docuSeal.UpdateSubmissionAsync(_updateClearExpirationRequest);
}
