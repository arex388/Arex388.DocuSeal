using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.UpdateTemplateDocuments;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint allows you to add, remove or replace documents in the template with provided PDF/DOCX file or HTML content.
/// </summary>
public static class UpdateTemplateDocuments {
	/// <summary>
	/// Update template documents request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The list of documents to add or replace in the template.
		/// </summary>
		public required IList<RequestDocument> Documents { get; init; } = [];

		internal string Endpoint => $"templates/{Id}/documents";

		/// <summary>
		/// The unique identifier of the documents template.
		/// </summary>
		[JsonIgnore]
		public TemplateId Id { get; init; }

		/// <summary>
		/// Set to `true` to merge all existing and new documents into a single PDF document in the template.
		/// </summary>
		[JsonPropertyName("merge")]
		public bool MustMerge { get; init; }
	}

	/// <summary>
	/// Update template documents request document.
	/// </summary>
	public sealed class RequestDocument {
		/// <summary>
		/// Base64-encoded content of the PDF or DOCX file or downloadable file URL. Leave it empty if you create a new document using HTML param.
		/// </summary>
		[JsonPropertyName("file")]
		public string? FileBase64 { get; init; }

		/// <summary>
		/// HTML template with field tags. Leave it empty if you add a document via PDF or DOCX base64 encoded file param or URL.
		/// </summary>
		public string? Html { get; init; }

		/// <summary>
		/// Set to `true` to replace existing document with a new file at `position`. Existing document fields will be transferred to the new document if it doesn't contain any fields
		/// </summary>
		[JsonPropertyName("replace")]
		public bool MustReplace { get; init; }

		/// <summary>
		/// Set to `true` to remove existing document at given `position` or with given `name`.
		/// </summary>
		[JsonPropertyName("remove")]
		public bool MustRemove { get; init; }

		/// <summary>
		/// Document name. Random uuid will be assigned when not specified.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// Position of the document. By default will be added as the last document in the template.
		/// </summary>
		public int Position { get; init; }
	}

	/// <summary>
	/// Update template documents response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response>;
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator(
		IValidator<RequestDocument> requestDocumentValidator) {
		RuleFor(r => r.Documents).ForEach(r => r.SetValidator(requestDocumentValidator)).NotEmpty();
		RuleFor(r => r.Id).NotEmpty();
	}
}

file sealed class RequestDocumentValidator :
	AbstractValidator<RequestDocument> {
	public RequestDocumentValidator() {
		RuleFor(r => r.FileBase64).NotEmpty().When(r => !r.Html.HasValue());
		RuleFor(r => r.Html).NotEmpty().When(r => !r.FileBase64.HasValue());
		RuleFor(r => r.Name).NotEmpty();
	}
}