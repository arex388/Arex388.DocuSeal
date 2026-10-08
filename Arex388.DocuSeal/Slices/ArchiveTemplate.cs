using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.ArchiveTemplate;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint allows you to archive a document template.
/// </summary>
public static class ArchiveTemplate {
	/// <summary>
	/// Archive template request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => $"templates/{Id}";

		/// <summary>
		/// The unique identifier of the document template.
		/// </summary>
		public required TemplateId Id { get; init; }
	}

	/// <summary>
	/// Archive template response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The archived template's archived timestamp.
		/// </summary>
		[JsonPropertyName("archived_at")]
		public DateTime? ArchivedAtUtc { get; init; }

		[JsonInclude]
		internal string? Error { get; init; }

		/// <summary>
		/// The archived template's id.
		/// </summary>
		public TemplateId? Id { get; init; }
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator() {
		RuleFor(r => r.Id).NotEmpty();
	}
}