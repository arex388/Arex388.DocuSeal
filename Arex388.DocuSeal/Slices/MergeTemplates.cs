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
		internal string Endpoint { get; } = "templates/merge";

		/// <summary>
		/// Your application-specific unique string key to identify the merged template within your app.
		/// </summary>
		[JsonPropertyName("external_id")]
		public string? ExternalId { get; init; }

		/// <summary>
		/// The name of the folder in which the merged template should be placed.
		/// </summary>
		[JsonPropertyName("folder_name")]
		public string? Folder { get; init; }

		/// <summary>
		/// Set to <see langword="true" /> to make the merged template available via a shared link, which allows anyone with the link to create a submission from it. The API defaults to <see langword="true" />.
		/// </summary>
		[JsonPropertyName("shared_link")]
		public bool? HasSharedLink { get; init; }

		/// <summary>
		/// An array of template ids to merge into a new template.
		/// </summary>
		[JsonPropertyName("template_ids")]
		public required IList<TemplateId> Ids { get; init; } = [];

		/// <summary>
		/// Template name. Existing name with (Merged) suffix will be used if not specified.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// The submitter role names to use in the merged template.
		/// </summary>
		public IList<string>? Roles { get; init; }
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