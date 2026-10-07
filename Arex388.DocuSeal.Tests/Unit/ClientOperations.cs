namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Every IDocuSealClient member, addressable by name so the response-contract
/// theories can run one row per operation with serializable data.
/// </summary>
internal static class ClientOperations {
	private const string _fileBase64 = "JVBERi0xLjQK";

	public static readonly TemplateId TemplateId = new(1001);
	public static readonly SubmissionId SubmissionId = new(2001);
	public static readonly SubmitterId SubmitterId = new(3001);

	/// <summary>
	/// Operations whose response carries a payload (<c>Template</c>, <c>Submission</c>, <c>Submitter</c>).
	/// </summary>
	public static readonly string[] WithPayload = [
		nameof(IDocuSealClient.CloneTemplateAsync),
		nameof(IDocuSealClient.CreateSubmissionAsync),
		nameof(IDocuSealClient.CreateTemplateAsync),
		CreateTemplateFromFile,
		nameof(IDocuSealClient.GetSubmissionAsync),
		nameof(IDocuSealClient.GetSubmitterAsync),
		nameof(IDocuSealClient.GetTemplateAsync),
		nameof(IDocuSealClient.MergeTemplatesAsync)
	];

	/// <summary>
	/// Operations whose response is only <c>Errors</c> / <c>Success</c>.
	/// </summary>
	public static readonly string[] WithoutPayload = [
		nameof(IDocuSealClient.ArchiveSubmissionAsync),
		nameof(IDocuSealClient.ArchiveTemplateAsync),
		nameof(IDocuSealClient.UpdateSubmitterAsync),
		nameof(IDocuSealClient.UpdateTemplateAsync),
		nameof(IDocuSealClient.UpdateTemplateDocumentsAsync)
	];

	/// <summary>
	/// The paged list operations.
	/// </summary>
	public static readonly string[] Lists = [
		nameof(IDocuSealClient.ListSubmissionsAsync),
		nameof(IDocuSealClient.ListSubmittersAsync),
		nameof(IDocuSealClient.ListTemplatesAsync)
	];

	public const string CreateTemplateFromFile = nameof(IDocuSealClient.CreateTemplateAsync) + "(FileInfo)";

	public static TheoryData<string> All => [
		.. WithPayload,
		.. WithoutPayload,
		.. Lists
	];

	public static TheoryData<string> AllWithPayload => [
		.. WithPayload
	];

	public static TheoryData<string> AllWithoutPayload => [
		.. WithoutPayload
	];

	public static TheoryData<string> AllLists => [
		.. Lists
	];

	public static Task<OperationResult> InvokeAsync(
		IDocuSealClient docuSeal,
		string operation,
		CancellationToken cancellationToken = default) => operation switch {
			nameof(IDocuSealClient.ArchiveSubmissionAsync) => ShapeAsync(docuSeal.ArchiveSubmissionAsync(SubmissionId, cancellationToken)),
			nameof(IDocuSealClient.ArchiveTemplateAsync) => ShapeAsync(docuSeal.ArchiveTemplateAsync(TemplateId, cancellationToken)),
			nameof(IDocuSealClient.CloneTemplateAsync) => ShapeAsync(docuSeal.CloneTemplateAsync(new CloneTemplate.Request {
				Id = TemplateId
			}, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.CreateSubmissionAsync) => ShapeAsync(docuSeal.CreateSubmissionAsync(new CreateSubmission.Request {
				Submitters = [
					new CreateSubmission.RequestSubmitter {
						Email = "signer1@example.com"
					}
				],
				TemplateId = TemplateId
			}, cancellationToken), r => r.Submission),
			nameof(IDocuSealClient.CreateTemplateAsync) => ShapeAsync(docuSeal.CreateTemplateAsync(new CreateTemplate.Request {
				Documents = [
					new CreateTemplate.RequestDocument {
						FileBase64 = _fileBase64,
						Name = "Test Document"
					}
				],
				Endpoint = CreateTemplate.Endpoints.Pdf,
				Name = "Test Template"
			}, cancellationToken), r => r.Template),
			CreateTemplateFromFile => ShapeAsync(docuSeal.CreateTemplateAsync(Utilities.DocuSealFile, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.GetSubmissionAsync) => ShapeAsync(docuSeal.GetSubmissionAsync(SubmissionId, cancellationToken), r => r.Submission),
			nameof(IDocuSealClient.GetSubmitterAsync) => ShapeAsync(docuSeal.GetSubmitterAsync(SubmitterId, cancellationToken), r => r.Submitter),
			nameof(IDocuSealClient.GetTemplateAsync) => ShapeAsync(docuSeal.GetTemplateAsync(TemplateId, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.ListSubmissionsAsync) => ShapeAsync(docuSeal.ListSubmissionsAsync(cancellationToken)),
			nameof(IDocuSealClient.ListSubmittersAsync) => ShapeAsync(docuSeal.ListSubmittersAsync(cancellationToken)),
			nameof(IDocuSealClient.ListTemplatesAsync) => ShapeAsync(docuSeal.ListTemplatesAsync(cancellationToken)),
			nameof(IDocuSealClient.MergeTemplatesAsync) => ShapeAsync(docuSeal.MergeTemplatesAsync(new MergeTemplates.Request {
				Ids = [
					TemplateId,
					new TemplateId(1002)
				]
			}, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.UpdateSubmitterAsync) => ShapeAsync(docuSeal.UpdateSubmitterAsync(new UpdateSubmitter.Request {
				Id = SubmitterId,
				Name = "Signer One"
			}, cancellationToken)),
			nameof(IDocuSealClient.UpdateTemplateAsync) => ShapeAsync(docuSeal.UpdateTemplateAsync(new UpdateTemplate.Request {
				Id = TemplateId,
				Name = "Renamed Template"
			}, cancellationToken)),
			nameof(IDocuSealClient.UpdateTemplateDocumentsAsync) => ShapeAsync(docuSeal.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
				Documents = [
					new UpdateTemplateDocuments.RequestDocument {
						FileBase64 = _fileBase64,
						Name = "Second Document"
					}
				],
				Id = TemplateId
			}, cancellationToken)),
			_ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown client operation.")
		};

	private static async Task<OperationResult> ShapeAsync<TResponse>(
		Task<TResponse> task,
		Func<TResponse, object?>? payload = null)
		where TResponse : ResponseBase<TResponse>, new() {
		var response = await task;

		return new OperationResult(response.Success, response.Errors, payload?.Invoke(response));
	}
}

/// <summary>
/// The response surface every operation shares, plus its payload when it has one.
/// </summary>
internal sealed record OperationResult(
	bool Success,
	IList<string> Errors,
	object? Payload);
