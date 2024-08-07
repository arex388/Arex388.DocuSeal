using Arex388.DocuSeal.Models;
using FluentValidation;
using static Arex388.DocuSeal.GetTemplate;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to retrieve information about a document template.
/// </summary>
public static class GetTemplate {
	/// <summary>
	/// Get template request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => $"templates/{Id}";

		/// <summary>
		/// The unique identifier of the document template.
		/// </summary>
		public required TemplateId Id { get; init; }
	}

	/// <summary>
	/// Get template response.
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
	public RequestValidator() {
		RuleFor(r => r.Id).NotEmpty();
	}
}