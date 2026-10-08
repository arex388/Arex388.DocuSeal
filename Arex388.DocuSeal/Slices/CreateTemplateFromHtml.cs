using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CreateTemplateFromHtml;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to generate a PDF document template from the provided HTML content, incorporating pre-defined fields. Use {{Field Name;role=Signer1;type=date}} text tags or the field tags (e.g. &lt;text-field&gt;) to define fillable fields in the HTML.
/// </summary>
public static class CreateTemplateFromHtml {
	/// <summary>
	/// Create template from HTML request. Set <see cref="Html" /> for a single-document template or <see cref="Documents" /> for a multi-document template; one of the two is required.
	/// </summary>
	public sealed class Request {
		internal string Endpoint { get; } = "templates/html";

		/// <summary>
		/// The documents built from HTML, for a template with multiple documents. Leave empty when using <see cref="Html" /> for a template with a single document.
		/// </summary>
		public IList<RequestDocument>? Documents { get; init; }

		/// <summary>
		/// Your application-specific unique string key to identify the template within your app. An existing template with this key is updated with the new HTML.
		/// </summary>
		[JsonPropertyName("external_id")]
		public string? ExternalId { get; init; }

		/// <summary>
		/// The folder's name in which the template should be created.
		/// </summary>
		[JsonPropertyName("folder_name")]
		public string? Folder { get; init; }

		/// <summary>
		/// Set to <see langword="true" /> to make the template available via a shared link, which allows anyone with the link to create a submission from it. The API defaults to <see langword="true" />.
		/// </summary>
		[JsonPropertyName("shared_link")]
		public bool? HasSharedLink { get; init; }

		/// <summary>
		/// The HTML template with field tags, for a template with a single document. Required unless <see cref="Documents" /> is set.
		/// </summary>
		public string? Html { get; init; }

		/// <summary>
		/// The HTML template of the footer to display on every page.
		/// </summary>
		[JsonPropertyName("html_footer")]
		public string? HtmlFooter { get; init; }

		/// <summary>
		/// The HTML template of the header to display on every page.
		/// </summary>
		[JsonPropertyName("html_header")]
		public string? HtmlHeader { get; init; }

		/// <summary>
		/// Name of the template. The API assigns a random uuid when not specified.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// The page size. The API defaults to <see cref="PageSize.Letter" />.
		/// </summary>
		public PageSize? Size { get; init; }
	}

	/// <summary>
	/// Create template from HTML request document.
	/// </summary>
	public sealed class RequestDocument {
		/// <summary>
		/// The HTML template with field tags.
		/// </summary>
		public required string Html { get; init; }

		/// <summary>
		/// Name of the document. The API assigns a random uuid when not specified.
		/// </summary>
		public string? Name { get; init; }
	}

	/// <summary>
	/// Create template from HTML response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The template.
		/// </summary>
		public Template? Template { get; init; }
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator(
		IValidator<RequestDocument> requestDocumentValidator) {
		RuleFor(r => r.Documents!).ForEach(r => r.SetValidator(requestDocumentValidator)).When(r => r.Documents is not null);
		RuleForEach(r => r.Documents!).NotNull().When(r => r.Documents is not null).WithMessage("'Documents' must not contain null entries.");
		RuleFor(r => r.Html).NotEmpty().When(r => r.Documents is null || r.Documents.All(d => d is null)).WithMessage("'Html' must not be empty unless 'Documents' contains at least one document.");
		RuleFor(r => r.Html).Must(h => string.IsNullOrWhiteSpace(h)).When(r => r.Documents is { Count: > 0 }).WithMessage("'Html' and 'Documents' cannot both be set.");
		RuleFor(r => r.Size).Must(s => s != PageSize.Unknown).When(r => r.Size is not null).WithMessage("'{PropertyName}' must not be empty.");
	}
}

file sealed class RequestDocumentValidator :
	AbstractValidator<RequestDocument> {
	public RequestDocumentValidator() {
		RuleFor(r => r.Html).NotEmpty();
	}
}