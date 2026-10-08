using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CreateTemplate;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to create a fillable document template for existing Microsoft Word or PDF document. Use {{Field Name;role=Signer1;type=date}} text tags to define fillable fields in the document.
/// </summary>
public static class CreateTemplate {
	/// <summary>
	/// Create template endpoints.
	/// </summary>
	public static class Endpoints {
		/// <summary>
		/// The endpoint for .docx files.
		/// </summary>
		public static readonly string Docx = "templates/docx";

		/// <summary>
		/// The endpoint for .pdf files.
		/// </summary>
		public static readonly string Pdf = "templates/pdf";
	}

	/// <summary>
	/// Create template request. The same request serves the PDF and DOCX endpoints; members documented as PDF-only or DOCX-only fail validation on the other endpoint.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The documents for the template.
		/// </summary>
		public required IList<RequestDocument> Documents { get; init; } = [];

		/// <summary>
		/// The endpoint to send the request to: <see cref="Endpoints.Pdf" /> or <see cref="Endpoints.Docx" />.
		/// </summary>
		[JsonIgnore]
		public string Endpoint { get; init; } = null!;

		/// <summary>
		/// Your application-specific unique string key to identify the template within your app. An existing template with this key is updated with the new documents.
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
		/// Set to <see langword="true" /> to remove the PDF form fields from the documents. The API defaults to <see langword="false" />. PDF only.
		/// </summary>
		[JsonPropertyName("flatten")]
		public bool? MustFlatten { get; init; }

		/// <summary>
		/// Set to <see langword="false" /> to keep the {{text}} tags in the PDF, which can be used along with transparent text tags for faster and more robust PDF processing. The API defaults to <see langword="true" />. PDF only.
		/// </summary>
		[JsonPropertyName("remove_tags")]
		public bool? MustRemoveTags { get; init; }

		/// <summary>
		/// Name of the template.
		/// </summary>
		public string? Name { get; init; }
	}

	/// <summary>
	/// Create template request document.
	/// </summary>
	public sealed class RequestDocument {
		/// <summary>
		/// Fields are optional if you use {{...}} text tags to define fields in the document.
		/// </summary>
		public IList<RequestDocumentField>? Fields { get; init; }

		/// <summary>
		/// Base64-encoded content of the PDF or DOCX file or downloadable file URL.
		/// </summary>
		[JsonPropertyName("file")]
		public required string FileBase64 { get; init; }

		/// <summary>
		/// Set to <see langword="true" /> to make the document dynamic, so its content can be edited or use [[variables]] in the template editor. The API defaults to <see langword="false" />. DOCX only.
		/// </summary>
		[JsonPropertyName("dynamic")]
		public bool? IsDynamic { get; init; }

		/// <summary>
		/// Name of the document.
		/// </summary>
		public required string Name { get; init; }
	}

	/// <summary>
	/// Create template request document field.
	/// </summary>
	public sealed class RequestDocumentField {
		/// <summary>
		/// The areas where the field is located in the document.
		/// </summary>
		public IList<RequestDocumentFieldArea>? Areas { get; init; }

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
		public required string Name { get; init; }

		/// <summary>
		/// The option values for a <see cref="FieldType.Select" /> field.
		/// </summary>
		public IList<string>? Options { get; init; }

		/// <summary>
		/// The field's display preferences.
		/// </summary>
		public RequestFieldPreferences? Preferences { get; init; }

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
		//[JsonConverter(typeof(FieldTypeJsonConverter))]
		public FieldType? Type { get; init; }

		/// <summary>
		/// The field's validation rules.
		/// </summary>
		public RequestFieldValidation? Validation { get; init; }
	}

	/// <summary>
	/// Create template request document field area.
	/// </summary>
	public sealed class RequestDocumentFieldArea {
		/// <summary>
		/// Height of the field area.
		/// </summary>
		[JsonPropertyName("h")]
		public required decimal Height { get; init; }

		/// <summary>
		/// The option value this area represents, for <see cref="FieldType.Radio" /> and <see cref="FieldType.Multiple" /> fields.
		/// </summary>
		public string? Option { get; init; }

		/// <summary>
		/// Page number of the field area. Starts from 1.
		/// </summary>
		public required int Page { get; init; }

		/// <summary>
		/// Width of the field area.
		/// </summary>
		[JsonPropertyName("w")]
		public required decimal Width { get; init; }

		/// <summary>
		/// X-coordinate of the field area.
		/// </summary>
		public required decimal X { get; init; }

		/// <summary>
		/// Y-coordinate of the field area.
		/// </summary>
		public required decimal Y { get; init; }
	}

	/// <summary>
	/// Create template response.
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
		RuleFor(r => r.Documents).ForEach(r => r.SetValidator(requestDocumentValidator)).NotEmpty();
		RuleFor(r => r.Documents).Must(d => d.All(rd => rd?.IsDynamic is null)).When(r => r.Endpoint == Endpoints.Pdf && r.Documents is not null).WithMessage("'Is Dynamic' is only supported when creating a template from a DOCX file.");
		RuleFor(r => r.Endpoint).NotEmpty();
		RuleFor(r => r.MustFlatten).Null().When(r => r.Endpoint == Endpoints.Docx).WithMessage("'{PropertyName}' is only supported when creating a template from a PDF file.");
		RuleFor(r => r.MustRemoveTags).Null().When(r => r.Endpoint == Endpoints.Docx).WithMessage("'{PropertyName}' is only supported when creating a template from a PDF file.");
	}
}

file sealed class RequestDocumentValidator :
	AbstractValidator<RequestDocument> {
	public RequestDocumentValidator(
		IValidator<RequestDocumentField> requestDocumentFieldValidator) {
		RuleFor(r => r.Fields!).ForEach(r => r.SetValidator(requestDocumentFieldValidator)).When(r => r.Fields is not null);
		RuleFor(r => r.FileBase64).NotEmpty();
		RuleFor(r => r.Name).NotEmpty();
	}
}

file sealed class RequestDocumentFieldValidator :
	AbstractValidator<RequestDocumentField> {
	public RequestDocumentFieldValidator(
		IValidator<RequestDocumentFieldArea> requestDocumentFieldAreaValidator,
		IValidator<RequestFieldPreferences> requestFieldPreferencesValidator,
		IValidator<RequestFieldValidation> requestFieldValidationValidator) {
		RuleFor(r => r.Areas!).ForEach(r => r.SetValidator(requestDocumentFieldAreaValidator)).When(r => r.Areas is not null);
		RuleFor(r => r.Name).NotEmpty();
		RuleFor(r => r.Preferences).SetValidator(requestFieldPreferencesValidator!);
		RuleFor(r => r.Type).Must(t => t != FieldType.Unknown).WithMessage("'{PropertyName}' must not be empty.");
		RuleFor(r => r.Validation).SetValidator(requestFieldValidationValidator!);
	}
}

file sealed class RequestDocumentFieldAreaValidator :
	AbstractValidator<RequestDocumentFieldArea> {
	public RequestDocumentFieldAreaValidator() {
		RuleFor(r => r.Height).NotEmpty();
		RuleFor(r => r.Page).NotEmpty();
		RuleFor(r => r.Width).NotEmpty();
		RuleFor(r => r.X).GreaterThanOrEqualTo(0);
		RuleFor(r => r.Y).GreaterThanOrEqualTo(0);
	}
}