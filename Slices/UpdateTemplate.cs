using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.UpdateTemplate;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to move a document template to a different folder and update the name of the template.
/// </summary>
public static class UpdateTemplate {
	/// <summary>
	/// Update template request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => $"templates/{Id}";

		/// <summary>
		/// The folder's name to which the template should be moved.
		/// </summary>
		[JsonPropertyName("folder_name")]
		public string? Folder { get; init; }

		/// <summary>
		/// The unique identifier of the document template.
		/// </summary>
		[JsonIgnore]
		public TemplateId Id { get; init; }

		/// <summary>
		/// Set `false` to unarchive template.
		/// </summary>
		[JsonPropertyName("archived")]
		public bool IsArchived { get; init; }

		/// <summary>
		/// The name of the template.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// An array of submitter role names to update the template with.
		/// </summary>
		public IList<string> Roles { get; init; } = [];
	}

	/// <summary>
	/// Update template response.
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