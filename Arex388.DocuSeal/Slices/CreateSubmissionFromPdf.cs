using FluentValidation;
using System.Globalization;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CreateSubmissionFromPdf;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to create a one-off submission request from a PDF, without a saved template. Use {{Field Name;role=Signer1;type=date}} text tags to define fillable fields in the document, or specify the exact coordinates of the document fields using <see cref="RequestDocument.Fields" />.
/// </summary>
public static class CreateSubmissionFromPdf {
	/// <summary>
	/// Create submission from PDF request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The PDF documents to create the submission from.
		/// </summary>
		public required IList<RequestDocument> Documents { get; init; } = [];

		internal string Endpoint { get; } = "submissions/pdf";

		//	The API takes expire_at as a string such as "2024-09-01 12:00:00 UTC", not as an ISO 8601 date-time.
		[JsonInclude, JsonPropertyName("expire_at")]
		internal string? ExpireAt => ExpireAtUtc?.AsUtc().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

		/// <summary>
		/// The date and time, in UTC, after which the submission becomes unavailable for signature. A value of <see cref="DateTimeKind.Local" /> kind is converted to UTC; a value of <see cref="DateTimeKind.Unspecified" /> kind is taken to already be UTC.
		/// </summary>
		[JsonIgnore]
		public DateTime? ExpireAtUtc { get; init; }

		/// <summary>
		/// The message for the submission. Unset is omitted from the request body; a message with neither member set is sent as <c>"message": {}</c>; the spec requires neither member.
		/// </summary>
		public CreateSubmission.RequestMessage? Message { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending. The API defaults to `true`.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool? MustEmail { get; init; }

		/// <summary>
		/// Set to <see langword="true" /> to remove the PDF form fields from the documents. The API defaults to <see langword="false" />.
		/// </summary>
		[JsonPropertyName("flatten")]
		public bool? MustFlatten { get; init; }

		/// <summary>
		/// Set to <see langword="true" /> to merge the documents into a single PDF file. The API defaults to <see langword="false" />.
		/// </summary>
		[JsonPropertyName("merge_documents")]
		public bool? MustMergeDocuments { get; init; }

		/// <summary>
		/// Set to <see langword="false" /> to keep the {{text}} tags in the PDF, which can be used along with transparent text tags for faster and more robust PDF processing. The API defaults to <see langword="true" />.
		/// </summary>
		[JsonPropertyName("remove_tags")]
		public bool? MustRemoveTags { get; init; }

		/// <summary>
		/// Set `true` to send signature request via phone number and SMS. The API defaults to `false`.
		/// </summary>
		[JsonPropertyName("send_sms")]
		public bool? MustSms { get; init; }

		/// <summary>
		/// Name of the submission.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// Specify BCC address to send signed documents to after the completion.
		/// </summary>
		[JsonPropertyName("bcc_completed")]
		public string? OnCompletedBccEmail { get; init; }

		/// <summary>
		/// Specify URL to redirect to after the submission completion.
		/// </summary>
		[JsonPropertyName("completed_redirect_url")]
		public string? OnCompletedUrl { get; init; }

		/// <summary>
		/// Pass 'random' to send signature request emails to all parties right away. The order is 'preserved' by default so the second party will receive a signature request email only after the document is signed by the first party.
		/// </summary>
		public SubmitterOrder? Order { get; init; }

		/// <summary>
		/// Specify Reply-To address to use in the notification emails.
		/// </summary>
		[JsonPropertyName("reply_to")]
		public string? ReplyToEmail { get; init; }

		/// <summary>
		/// The list of submitters for the submission.
		/// </summary>
		public required IList<CreateSubmission.RequestSubmitter> Submitters { get; init; } = [];

		/// <summary>
		/// The ids of templates to use in the submission along with the provided documents, to create a multi-document submission when some of the required documents exist within templates.
		/// </summary>
		[JsonPropertyName("template_ids")]
		public IList<TemplateId>? TemplateIds { get; init; }
	}

	/// <summary>
	/// Create submission from PDF request document.
	/// </summary>
	public sealed class RequestDocument {
		/// <summary>
		/// Fields are optional if you use {{...}} text tags to define fields in the document.
		/// </summary>
		public IList<RequestDocumentField>? Fields { get; init; }

		/// <summary>
		/// Base64-encoded content of the PDF file or downloadable file URL.
		/// </summary>
		[JsonPropertyName("file")]
		public required string FileBase64 { get; init; }

		/// <summary>
		/// Name of the document.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// The document's position in the submission. When not set, the document is added in the order it appears in <see cref="Request.Documents" />.
		/// </summary>
		public int? Position { get; init; }
	}

	/// <summary>
	/// Create submission from PDF request document field. Every member is optional; unlike a template field, a one-off field takes no display preferences or validation rules.
	/// </summary>
	public sealed class RequestDocumentField {
		/// <summary>
		/// The areas where the field is located in the document.
		/// </summary>
		public IList<CreateTemplate.RequestDocumentFieldArea>? Areas { get; init; }

		/// <summary>
		/// Field description displayed on the signing form. Supports Markdown.
		/// </summary>
		public string? Description { get; init; }

		/// <summary>
		/// Flag indicating if the field is required.
		/// </summary>
		[JsonPropertyName("required")]
		public bool? IsRequired { get; init; }

		/// <summary>
		/// Name of the field.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// The option values for a <see cref="FieldType.Select" /> field.
		/// </summary>
		public IList<string>? Options { get; init; }

		/// <summary>
		/// Role name of the signer.
		/// </summary>
		public string? Role { get; init; }

		/// <summary>
		/// Field title displayed on the signing form instead of the name. Supports Markdown.
		/// </summary>
		public string? Title { get; init; }

		/// <summary>
		/// Type of the field (e.g., text, signature, date, initials).
		/// </summary>
		public FieldType? Type { get; init; }
	}

	/// <summary>
	/// Create submission from PDF response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The created submission, with its submitters. The API returns a subset of the submission: it carries no documents, events, slug, or template, and no updated timestamp, so <see cref="Submission.UpdatedAtUtc" /> reads as <see langword="default" />.
		/// </summary>
		public Submission? Submission { get; init; }
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator(
		IValidator<CreateSubmission.RequestSubmitter> requestSubmitterValidator,
		IValidator<RequestDocument> requestDocumentValidator) {
		RuleFor(r => r.Documents).ForEach(r => r.NotNull().WithMessage("'Documents' must not contain null entries.").SetValidator(requestDocumentValidator)).NotEmpty();
		RuleFor(r => r.OnCompletedBccEmail).EmailAddress().When(r => r.OnCompletedBccEmail.HasValue());
		RuleFor(r => r.Order).Must(o => o != SubmitterOrder.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
		RuleFor(r => r.ReplyToEmail).EmailAddress().When(r => r.ReplyToEmail.HasValue());
		RuleFor(r => r.Submitters).ForEach(r => r.NotNull().WithMessage("'Submitters' must not contain null entries.").SetValidator(requestSubmitterValidator)).NotEmpty();
		RuleForEach(r => r.TemplateIds!).NotEmpty().When(r => r.TemplateIds is not null);
	}
}

file sealed class RequestDocumentValidator :
	AbstractValidator<RequestDocument> {
	public RequestDocumentValidator(
		IValidator<RequestDocumentField> requestDocumentFieldValidator) {
		RuleFor(r => r.Fields!).ForEach(r => r.NotNull().WithMessage("'Fields' must not contain null entries.").SetValidator(requestDocumentFieldValidator)).When(r => r.Fields is not null);
		RuleFor(r => r.FileBase64).NotEmpty();
		RuleFor(r => r.Name).NotEmpty();
		RuleFor(r => r.Position).GreaterThanOrEqualTo(0).When(r => r.Position.HasValue);
	}
}

file sealed class RequestDocumentFieldValidator :
	AbstractValidator<RequestDocumentField> {
	public RequestDocumentFieldValidator(
		IValidator<CreateTemplate.RequestDocumentFieldArea> requestDocumentFieldAreaValidator) {
		RuleFor(r => r.Areas!).ForEach(r => r.NotNull().WithMessage("'Areas' must not contain null entries.").SetValidator(requestDocumentFieldAreaValidator)).When(r => r.Areas is not null);
		RuleFor(r => r.Type).Must(t => t != FieldType.Unknown).WithMessage("'{PropertyName}' must not be empty.");
	}
}