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
	/// Operations whose response carries a payload (a <c>Template</c>, <c>Submission</c>,
	/// <c>Submitter</c>, or an id), which is null unless the call succeeded.
	/// </summary>
	public static readonly string[] WithPayload = [
		nameof(IDocuSealClient.ArchiveSubmissionAsync),
		nameof(IDocuSealClient.ArchiveTemplateAsync),
		nameof(IDocuSealClient.CloneTemplateAsync),
		nameof(IDocuSealClient.CreateSubmissionAsync),
		nameof(IDocuSealClient.CreateSubmissionFromDocxAsync),
		nameof(IDocuSealClient.CreateSubmissionFromEmailsAsync),
		nameof(IDocuSealClient.CreateSubmissionFromHtmlAsync),
		nameof(IDocuSealClient.CreateSubmissionFromPdfAsync),
		nameof(IDocuSealClient.CreateTemplateAsync),
		CreateTemplateFromFile,
		nameof(IDocuSealClient.CreateTemplateFromHtmlAsync),
		nameof(IDocuSealClient.GetSubmissionAsync),
		nameof(IDocuSealClient.GetSubmissionDocumentsAsync),
		nameof(IDocuSealClient.GetSubmitterAsync),
		nameof(IDocuSealClient.GetTemplateAsync),
		nameof(IDocuSealClient.MergeTemplatesAsync),
		nameof(IDocuSealClient.UpdateSubmissionAsync),
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

	/// <summary>
	/// Operations whose only public overload takes a value-type id, so no null
	/// argument can reach them. Every other operation takes a request or a file.
	/// </summary>
	public static readonly string[] IdOnly = [
		nameof(IDocuSealClient.ArchiveSubmissionAsync),
		nameof(IDocuSealClient.ArchiveTemplateAsync),
		nameof(IDocuSealClient.GetSubmissionAsync),
		nameof(IDocuSealClient.GetSubmitterAsync),
		nameof(IDocuSealClient.GetTemplateAsync)
	];

	public const string CreateTemplateFromFile = nameof(IDocuSealClient.CreateTemplateAsync) + "(FileInfo)";

	public static TheoryData<string> All => [
		.. WithPayload,
		.. Lists
	];

	public static TheoryData<string> AllIdOnly => [
		.. IdOnly
	];

	public static TheoryData<string> AllWithRequest => [
		.. WithPayload.Concat(Lists).Except(IdOnly)
	];

	public static TheoryData<string> AllWithPayload => [
		.. WithPayload
	];

	public static TheoryData<string> AllLists => [
		.. Lists
	];

	public static Task<OperationResult> InvokeAsync(
		IDocuSealClient docuSeal,
		string operation,
		CancellationToken cancellationToken = default) => operation switch {
			nameof(IDocuSealClient.ArchiveSubmissionAsync) => ShapeAsync(docuSeal.ArchiveSubmissionAsync(SubmissionId, cancellationToken), r => r.Id),
			nameof(IDocuSealClient.ArchiveTemplateAsync) => ShapeAsync(docuSeal.ArchiveTemplateAsync(TemplateId, cancellationToken), r => r.Id),
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
			}, cancellationToken), r => r.SubmissionId),
			nameof(IDocuSealClient.CreateSubmissionFromDocxAsync) => ShapeAsync(docuSeal.CreateSubmissionFromDocxAsync(new CreateSubmissionFromDocx.Request {
				Documents = [
					new CreateSubmissionFromDocx.RequestDocument {
						FileBase64 = _fileBase64,
						Name = "Test Document"
					}
				],
				Submitters = [
					new CreateSubmission.RequestSubmitter {
						Email = "signer1@example.com"
					}
				]
			}, cancellationToken), r => r.Submission),
			nameof(IDocuSealClient.CreateSubmissionFromEmailsAsync) => ShapeAsync(docuSeal.CreateSubmissionFromEmailsAsync(new CreateSubmissionFromEmails.Request {
				Emails = [
					"signer1@example.com",
					"signer2@example.com"
				],
				TemplateId = TemplateId
			}, cancellationToken), r => r.Submitters.Count > 0 ? r.Submitters : null),
			nameof(IDocuSealClient.CreateSubmissionFromHtmlAsync) => ShapeAsync(docuSeal.CreateSubmissionFromHtmlAsync(new CreateSubmissionFromHtml.Request {
				Documents = [
					new CreateSubmissionFromHtml.RequestDocument {
						Html = "<p>Test Document</p>"
					}
				],
				Submitters = [
					new CreateSubmission.RequestSubmitter {
						Email = "signer1@example.com"
					}
				]
			}, cancellationToken), r => r.Submission),
			nameof(IDocuSealClient.CreateSubmissionFromPdfAsync) => ShapeAsync(docuSeal.CreateSubmissionFromPdfAsync(new CreateSubmissionFromPdf.Request {
				Documents = [
					new CreateSubmissionFromPdf.RequestDocument {
						FileBase64 = _fileBase64,
						Name = "Test Document"
					}
				],
				Submitters = [
					new CreateSubmission.RequestSubmitter {
						Email = "signer1@example.com"
					}
				]
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
			nameof(IDocuSealClient.CreateTemplateFromHtmlAsync) => ShapeAsync(docuSeal.CreateTemplateFromHtmlAsync(new CreateTemplateFromHtml.Request {
				Html = "<p>Test Document</p>",
				Name = "Test Template"
			}, cancellationToken), r => r.Template),
			CreateTemplateFromFile => ShapeAsync(docuSeal.CreateTemplateAsync(Utilities.DocuSealFile, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.GetSubmissionAsync) => ShapeAsync(docuSeal.GetSubmissionAsync(SubmissionId, cancellationToken), r => r.Submission),
			nameof(IDocuSealClient.GetSubmissionDocumentsAsync) => ShapeAsync(docuSeal.GetSubmissionDocumentsAsync(SubmissionId, cancellationToken), r => r.Id),
			nameof(IDocuSealClient.GetSubmitterAsync) => ShapeAsync(docuSeal.GetSubmitterAsync(SubmitterId, cancellationToken), r => r.Submitter),
			nameof(IDocuSealClient.GetTemplateAsync) => ShapeAsync(docuSeal.GetTemplateAsync(TemplateId, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.ListSubmissionsAsync) => ShapeAsync(docuSeal.ListSubmissionsAsync(cancellationToken), r => new ListPayload(r.Pagination, [.. r.Submissions])),
			nameof(IDocuSealClient.ListSubmittersAsync) => ShapeAsync(docuSeal.ListSubmittersAsync(cancellationToken), r => new ListPayload(r.Pagination, [.. r.Submitters])),
			nameof(IDocuSealClient.ListTemplatesAsync) => ShapeAsync(docuSeal.ListTemplatesAsync(cancellationToken), r => new ListPayload(r.Pagination, [.. r.Templates])),
			nameof(IDocuSealClient.MergeTemplatesAsync) => ShapeAsync(docuSeal.MergeTemplatesAsync(new MergeTemplates.Request {
				Ids = [
					TemplateId,
					new TemplateId(1002)
				]
			}, cancellationToken), r => r.Template),
			nameof(IDocuSealClient.UpdateSubmissionAsync) => ShapeAsync(docuSeal.UpdateSubmissionAsync(new UpdateSubmission.Request {
				Id = SubmissionId,
				Name = "Renamed Submission"
			}, cancellationToken), r => r.Submission),
			nameof(IDocuSealClient.UpdateSubmitterAsync) => ShapeAsync(docuSeal.UpdateSubmitterAsync(new UpdateSubmitter.Request {
				Id = SubmitterId,
				Name = "Signer One"
			}, cancellationToken), r => r.Submitter),
			nameof(IDocuSealClient.UpdateTemplateAsync) => ShapeAsync(docuSeal.UpdateTemplateAsync(new UpdateTemplate.Request {
				Id = TemplateId,
				Name = "Renamed Template"
			}, cancellationToken), r => r.Id),
			nameof(IDocuSealClient.UpdateTemplateDocumentsAsync) => ShapeAsync(docuSeal.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
				Documents = [
					new UpdateTemplateDocuments.RequestDocument {
						FileBase64 = _fileBase64,
						Name = "Second Document"
					}
				],
				Id = TemplateId
			}, cancellationToken), r => r.Template),
			_ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown client operation.")
		};

	/// <summary>
	/// Calls one of the <see cref="AllWithRequest" /> operations with a null request,
	/// or a null file for <see cref="CreateTemplateFromFile" />.
	/// </summary>
	public static Task<OperationResult> InvokeWithNullAsync(
		IDocuSealClient docuSeal,
		string operation) => operation switch {
			nameof(IDocuSealClient.CloneTemplateAsync) => ShapeAsync(docuSeal.CloneTemplateAsync(null!), r => r.Template),
			nameof(IDocuSealClient.CreateSubmissionAsync) => ShapeAsync(docuSeal.CreateSubmissionAsync(null!), r => r.SubmissionId),
			nameof(IDocuSealClient.CreateSubmissionFromDocxAsync) => ShapeAsync(docuSeal.CreateSubmissionFromDocxAsync(null!), r => r.Submission),
			nameof(IDocuSealClient.CreateSubmissionFromEmailsAsync) => ShapeAsync(docuSeal.CreateSubmissionFromEmailsAsync(null!), r => r.Submitters.Count > 0 ? r.Submitters : null),
			nameof(IDocuSealClient.CreateSubmissionFromHtmlAsync) => ShapeAsync(docuSeal.CreateSubmissionFromHtmlAsync(null!), r => r.Submission),
			nameof(IDocuSealClient.CreateSubmissionFromPdfAsync) => ShapeAsync(docuSeal.CreateSubmissionFromPdfAsync(null!), r => r.Submission),
			nameof(IDocuSealClient.CreateTemplateAsync) => ShapeAsync(docuSeal.CreateTemplateAsync((CreateTemplate.Request)null!), r => r.Template),
			CreateTemplateFromFile => ShapeAsync(docuSeal.CreateTemplateAsync((FileInfo)null!), r => r.Template),
			nameof(IDocuSealClient.CreateTemplateFromHtmlAsync) => ShapeAsync(docuSeal.CreateTemplateFromHtmlAsync(null!), r => r.Template),
			nameof(IDocuSealClient.GetSubmissionDocumentsAsync) => ShapeAsync(docuSeal.GetSubmissionDocumentsAsync((GetSubmissionDocuments.Request)null!), r => r.Id),
			nameof(IDocuSealClient.ListSubmissionsAsync) => ShapeAsync(docuSeal.ListSubmissionsAsync(null!), r => new ListPayload(r.Pagination, [.. r.Submissions])),
			nameof(IDocuSealClient.ListSubmittersAsync) => ShapeAsync(docuSeal.ListSubmittersAsync(null!), r => new ListPayload(r.Pagination, [.. r.Submitters])),
			nameof(IDocuSealClient.ListTemplatesAsync) => ShapeAsync(docuSeal.ListTemplatesAsync(null!), r => new ListPayload(r.Pagination, [.. r.Templates])),
			nameof(IDocuSealClient.MergeTemplatesAsync) => ShapeAsync(docuSeal.MergeTemplatesAsync(null!), r => r.Template),
			nameof(IDocuSealClient.UpdateSubmissionAsync) => ShapeAsync(docuSeal.UpdateSubmissionAsync(null!), r => r.Submission),
			nameof(IDocuSealClient.UpdateSubmitterAsync) => ShapeAsync(docuSeal.UpdateSubmitterAsync(null!), r => r.Submitter),
			nameof(IDocuSealClient.UpdateTemplateAsync) => ShapeAsync(docuSeal.UpdateTemplateAsync(null!), r => r.Id),
			nameof(IDocuSealClient.UpdateTemplateDocumentsAsync) => ShapeAsync(docuSeal.UpdateTemplateDocumentsAsync(null!), r => r.Template),
			_ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Not an operation that takes a request.")
		};

	/// <summary>
	/// Calls one of the <see cref="AllIdOnly" /> operations with the default (empty) id,
	/// the nearest those overloads come to a null argument.
	/// </summary>
	public static Task<OperationResult> InvokeWithDefaultIdAsync(
		IDocuSealClient docuSeal,
		string operation) => operation switch {
			nameof(IDocuSealClient.ArchiveSubmissionAsync) => ShapeAsync(docuSeal.ArchiveSubmissionAsync(default), r => r.Id),
			nameof(IDocuSealClient.ArchiveTemplateAsync) => ShapeAsync(docuSeal.ArchiveTemplateAsync(default), r => r.Id),
			nameof(IDocuSealClient.GetSubmissionAsync) => ShapeAsync(docuSeal.GetSubmissionAsync(default), r => r.Submission),
			nameof(IDocuSealClient.GetSubmitterAsync) => ShapeAsync(docuSeal.GetSubmitterAsync(default), r => r.Submitter),
			nameof(IDocuSealClient.GetTemplateAsync) => ShapeAsync(docuSeal.GetTemplateAsync(default), r => r.Template),
			_ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Not an id-only operation.")
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

/// <summary>
/// A list operation's payload: its pagination and its data items. A list
/// response is never null, so "no payload" for a list means no items and the
/// default pagination.
/// </summary>
internal sealed record ListPayload(
	ResponsePagination Pagination,
	IList<object> Items);
