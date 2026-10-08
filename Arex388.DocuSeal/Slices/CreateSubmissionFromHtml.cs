using FluentValidation;
using System.Globalization;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CreateSubmissionFromHtml;

namespace Arex388.DocuSeal;

/// <summary>
/// This API endpoint allows you to create a one-off submission request from the provided HTML content, without a saved template, with field tags (e.g. &lt;text-field&gt;) rendered as a fillable and signable form.
/// </summary>
public static class CreateSubmissionFromHtml {
	/// <summary>
	/// Create submission from HTML request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The documents built from HTML to create the submission from.
		/// </summary>
		public required IList<RequestDocument> Documents { get; init; } = [];

		internal string Endpoint { get; } = "submissions/html";

		//	The API takes expire_at as a string such as "2024-09-01 12:00:00 UTC", not as an ISO 8601 date-time.
		[JsonInclude, JsonPropertyName("expire_at")]
		internal string? ExpireAt => ExpireAtUtc?.AsUtc().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

		/// <summary>
		/// The date and time, in UTC, after which the submission becomes unavailable for signature. A value of <see cref="DateTimeKind.Local" /> kind is converted to UTC; a value of <see cref="DateTimeKind.Unspecified" /> kind is taken to already be UTC.
		/// </summary>
		[JsonIgnore]
		public DateTime? ExpireAtUtc { get; init; }

		/// <summary>
		/// The message for the submission.
		/// </summary>
		public CreateSubmission.RequestMessage? Message { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending. The API defaults to `true`.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool? MustEmail { get; init; }

		/// <summary>
		/// Set to <see langword="true" /> to merge the documents into a single PDF file. The API defaults to <see langword="false" />.
		/// </summary>
		[JsonPropertyName("merge_documents")]
		public bool? MustMergeDocuments { get; init; }

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
	/// Create submission from HTML request document.
	/// </summary>
	public sealed class RequestDocument {
		/// <summary>
		/// The HTML document content with field tags.
		/// </summary>
		public required string Html { get; init; }

		/// <summary>
		/// The HTML content of the footer to display on every page.
		/// </summary>
		[JsonPropertyName("html_footer")]
		public string? HtmlFooter { get; init; }

		/// <summary>
		/// The HTML content of the header to display on every page.
		/// </summary>
		[JsonPropertyName("html_header")]
		public string? HtmlHeader { get; init; }

		/// <summary>
		/// Name of the document. The API assigns a random uuid when not specified.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// The document's position in the submission. When not set, the document is added in the order it appears in <see cref="Request.Documents" />.
		/// </summary>
		public int? Position { get; init; }

		/// <summary>
		/// The page size. The API defaults to <see cref="PageSize.Letter" />.
		/// </summary>
		public PageSize? Size { get; init; }
	}

	/// <summary>
	/// Create submission from HTML response.
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
		IValidator<CreateSubmission.RequestMessage> requestMessageValidator,
		IValidator<CreateSubmission.RequestSubmitter> requestSubmitterValidator,
		IValidator<RequestDocument> requestDocumentValidator) {
		RuleFor(r => r.Documents).ForEach(r => r.NotNull().WithMessage("'Documents' must not contain null entries.").SetValidator(requestDocumentValidator)).NotEmpty();
		RuleFor(r => r.Message).SetValidator(requestMessageValidator!);
		RuleFor(r => r.OnCompletedBccEmail).EmailAddress().When(r => r.OnCompletedBccEmail.HasValue());
		RuleFor(r => r.Order).Must(o => o != SubmitterOrder.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
		RuleFor(r => r.ReplyToEmail).EmailAddress().When(r => r.ReplyToEmail.HasValue());
		RuleFor(r => r.Submitters).ForEach(r => r.NotNull().WithMessage("'Submitters' must not contain null entries.").SetValidator(requestSubmitterValidator)).NotEmpty();
		RuleForEach(r => r.TemplateIds!).NotEmpty().When(r => r.TemplateIds is not null);
	}
}

file sealed class RequestDocumentValidator :
	AbstractValidator<RequestDocument> {
	public RequestDocumentValidator() {
		RuleFor(r => r.Html).NotEmpty();
		RuleFor(r => r.Position).GreaterThanOrEqualTo(0).When(r => r.Position.HasValue);
		RuleFor(r => r.Size).Must(s => s != PageSize.Unknown).When(r => r.Size is not null).WithMessage("'{PropertyName}' must not be unknown.");
	}
}