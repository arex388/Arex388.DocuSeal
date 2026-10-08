using FluentValidation;
using System.Globalization;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.ListTemplates;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the ability to retrieve a list of available document templates.
/// </summary>
public static class ListTemplates {
	/// <summary>
	/// List templates request.
	/// </summary>
	public sealed class Request {
		internal static readonly Request Instance = new();
		internal string Endpoint => GetEndpoint(this);

		/// <summary>
		/// Get only templates with an id greater than this one. Pass the id from <see cref="ResponsePagination.Next" /> to load the next page.
		/// </summary>
		public TemplateId? After { get; init; }

		/// <summary>
		/// Get only templates with an id less than this one. Pass the id from <see cref="ResponsePagination.Previous" /> to load the previous page.
		/// </summary>
		public TemplateId? Before { get; init; }

		/// <summary>
		/// Filter templates by the application-specific identifier provided for the template via the API or the embedded template form builder.
		/// </summary>
		public string? ExternalId { get; init; }

		/// <summary>
		/// Filter templates by folder name.
		/// </summary>
		public string? Folder { get; init; }

		/// <summary>
		/// Get only archived templates instead of active ones.
		/// </summary>
		public bool IsArchived { get; init; }

		/// <summary>
		/// Get only templates shared with test mode.
		/// </summary>
		public bool IsShared { get; init; }

		/// <summary>
		/// Filter templates based on the name partial match.
		/// </summary>
		public string? Search { get; init; }

		/// <summary>
		/// Filter templates by unique slug.
		/// </summary>
		public string? Slug { get; init; }

		/// <summary>
		/// The number of templates to return. Default value is 10. Maximum value is 100.
		/// </summary>
		public int Take { get; init; } = 10;

		//	========================================================================
		//	Utilities
		//	========================================================================

		private static string GetEndpoint(
			Request request) {
			//	The parameters are appended in a fixed order to a cached builder, with no intermediate set, list or join.
			var endpoint = StringBuilderCache.Acquire().Append("templates?limit=").Append(request.Take);

			if (request.Folder.HasValue()) {
				endpoint.Append("&folder=").Append(Uri.EscapeDataString(request.Folder));
			}

			if (request.IsArchived) {
				endpoint.Append("&archived=true");
			}

			if (request.Search.HasValue()) {
				endpoint.Append("&q=").Append(Uri.EscapeDataString(request.Search));
			}

			if (request.Slug.HasValue()) {
				endpoint.Append("&slug=").Append(Uri.EscapeDataString(request.Slug));
			}

			if (request.ExternalId.HasValue()) {
				endpoint.Append("&external_id=").Append(Uri.EscapeDataString(request.ExternalId));
			}

			if (request.IsShared) {
				endpoint.Append("&shared=true");
			}

			if (request.After.HasValue) {
				endpoint.Append("&after=").Append(request.After.Value.Value.ToString(CultureInfo.InvariantCulture));
			}

			if (request.Before.HasValue) {
				endpoint.Append("&before=").Append(request.Before.Value.Value.ToString(CultureInfo.InvariantCulture));
			}

			return StringBuilderCache.GetStringAndRelease(endpoint);
		}
	}

	/// <summary>
	/// List templates response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		[JsonInclude]
		internal string? Error { get; init; }

		/// <summary>
		/// The response's pagination details.
		/// </summary>
		public ResponsePagination Pagination { get; init; } = ResponsePagination.Empty;

		/// <summary>
		/// The templates.
		/// </summary>
		[JsonPropertyName("data")]
		public IList<Template> Templates { get; init; } = [];
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator() {
		RuleFor(r => r.Take).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
	}
}