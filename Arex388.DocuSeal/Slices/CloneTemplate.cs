using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CloneTemplate;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint allows you to clone existing template into a new template.
/// </summary>
public static class CloneTemplate {
	/// <summary>
	/// Clone template request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The folder's name to which the template should be cloned.
		/// </summary>
		[JsonPropertyName("folder_name")]
		public string? Folder { get; init; }

		internal string Endpoint => $"templates/{Id}/clone";

		/// <summary>
		/// The unique identifier of the documents template.
		/// </summary>
		public required TemplateId Id { get; init; }

		/// <summary>
		/// Template name. Existing name with (Clone) suffix will be used if not specified.
		/// </summary>
		public string? Name { get; init; }
	}

	/// <summary>
	/// Clone template response.
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