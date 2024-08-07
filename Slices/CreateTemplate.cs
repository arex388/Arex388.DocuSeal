using Arex388.DocuSeal.Converters;
using Arex388.DocuSeal.Models;
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
	/// Create template request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// For internal use only.
		/// </summary>
		[JsonIgnore]
		public string Endpoint { get; init; } = null!;

		/// <summary>
		/// The documents for the template.
		/// </summary>
		public required IList<RequestDocument> Documents { get; init; } = [];

		/// <summary>
		/// Name of the template.
		/// </summary>
		public required string Name { get; init; }
	}

	/// <summary>
	/// Create template request document.
	/// </summary>
	public sealed class RequestDocument {
		/// <summary>
		/// Fields are optional if you use {{...}} text tags to define fields in the document.
		/// </summary>
		public IList<RequestDocumentField> Fields { get; init; } = [];

		/// <summary>
		/// Base64-encoded content of the PDF file or downloadable file URL.
		/// </summary>
		[JsonPropertyName("file")]
		public required string FileBase64 { get; init; }

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
		/// The areas for the field.
		/// </summary>
		public required IList<RequestDocumentFieldArea> Areas { get; init; } = [];

		/// <summary>
		/// Name of the field.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// Role name of the signer.
		/// </summary>
		public required string Role { get; init; }

		/// <summary>
		/// Type of the field (e.g., text, signature, date, initials).
		/// </summary>
		[JsonConverter(typeof(FieldTypeJsonConverter))]
		public required FieldType Type { get; init; }
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
		RuleFor(r => r.Endpoint).NotEmpty();
		RuleFor(r => r.Name).NotEmpty();
	}
}

file sealed class RequestDocumentValidator :
	AbstractValidator<RequestDocument> {
	public RequestDocumentValidator(
		IValidator<RequestDocumentField> requestDocumentFieldValidator) {
		RuleFor(r => r.Fields).ForEach(r => r.SetValidator(requestDocumentFieldValidator));
		RuleFor(r => r.FileBase64).NotEmpty();
		RuleFor(r => r.Name).NotEmpty();
	}
}

file sealed class RequestDocumentFieldValidator :
	AbstractValidator<RequestDocumentField> {
	public RequestDocumentFieldValidator(
		IValidator<RequestDocumentFieldArea> requestDocumentFieldAreaValidator) {
		RuleFor(r => r.Areas).ForEach(r => r.SetValidator(requestDocumentFieldAreaValidator));
		RuleFor(r => r.Name).NotEmpty();
		RuleFor(r => r.Role).NotEmpty();
		RuleFor(r => r.Type).NotEmpty();
	}
}

file sealed class RequestDocumentFieldAreaValidator :
	AbstractValidator<RequestDocumentFieldArea> {
	public RequestDocumentFieldAreaValidator() {
		RuleFor(r => r.Height).NotEmpty();
		RuleFor(r => r.Page).NotEmpty();
		RuleFor(r => r.Width).NotEmpty();
		RuleFor(r => r.X).NotEmpty();
		RuleFor(r => r.Y).NotEmpty();
	}
}