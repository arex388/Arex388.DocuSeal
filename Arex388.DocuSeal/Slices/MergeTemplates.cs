using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.MergeTemplates;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint allows you to merge multiple templates with documents and fields into a new combined template.
/// </summary>
public static class MergeTemplates {
	/// <summary>
	/// Merge template request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => "templates/merge";

		/// <summary>
		/// The name of the folder in which the merged template should be placed.
		/// </summary>
		[JsonPropertyName("folder_name")]
		public string? Folder { get; init; }

		/// <summary>
		/// An array of template ids to merge into a new template.
		/// </summary>
		[JsonPropertyName("template_ids")]
		public required IList<TemplateId> Ids { get; init; } = [];

		/// <summary>
		/// Template name. Existing name with (Merged) suffix will be used if not specified.
		/// </summary>
		public string? Name { get; init; }
	}

	/// <summary>
	/// Merge template response.
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
		RuleFor(r => r.Ids).NotEmpty();
	}
}