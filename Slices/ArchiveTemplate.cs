using FluentValidation;
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
		ResponseBase<Response>;
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