using Arex388.DocuSeal.Converters;
using FluentValidation;
using System.Globalization;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.ListSubmissions;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the ability to retrieve a list of available submissions.
/// </summary>
public static class ListSubmissions {
	/// <summary>
	/// List submissions request.
	/// </summary>
	public sealed class Request {
		internal static readonly Request Instance = new();

		internal string Endpoint => GetEndpoint(this);

		/// <summary>
		/// Get only submissions with an id greater than this one. Pass the id from <see cref="ResponsePagination.Next" /> to load the next page.
		/// </summary>
		public SubmissionId? After { get; init; }

		/// <summary>
		/// Get only submissions with an id less than this one. Pass the id from <see cref="ResponsePagination.Previous" /> to load the previous page.
		/// </summary>
		public SubmissionId? Before { get; init; }

		/// <summary>
		/// Filter submissions by template folder name.
		/// </summary>
		public string? Folder { get; init; }

		/// <summary>
		/// Set <see langword="true" /> to get only archived submissions, or <see langword="false" /> to get only active ones. Leave unset to apply no archive filter.
		/// </summary>
		public bool? IsArchived { get; init; }

		/// <summary>
		/// Filter submissions based on submitters name, email or phone partial match.
		/// </summary>
		public string? Search { get; init; }

		/// <summary>
		/// Filter submissions by unique slug.
		/// </summary>
		public string? Slug { get; init; }

		/// <summary>
		/// Filter submissions by status.
		/// </summary>
		public SubmissionStatus? Status { get; init; }

		/// <summary>
		/// The number of submissions to return. Default value is 10. Maximum value is 100.
		/// </summary>
		public int Take { get; init; } = 10;

		/// <summary>
		/// The template ID allows you to receive only the submissions created from that specific template.
		/// </summary>
		public TemplateId? TemplateId { get; init; }

		//	========================================================================
		//	Utilities
		//	========================================================================

		private static string GetEndpoint(
			Request request) {
			//	The parameters are appended in a fixed order to a cached builder, with no intermediate set, list or join.
			var endpoint = StringBuilderCache.Acquire().Append("submissions?limit=").Append(request.Take);

			if (request.Folder.HasValue()) {
				endpoint.Append("&template_folder=").Append(Uri.EscapeDataString(request.Folder));
			}

			if (request.Search.HasValue()) {
				endpoint.Append("&q=").Append(Uri.EscapeDataString(request.Search));
			}

			if (request.TemplateId.HasValue) {
				endpoint.Append("&template_id=").Append(request.TemplateId.Value.Value.ToString(CultureInfo.InvariantCulture));
			}

			if (request.Status.HasValue) {
				endpoint.Append("&status=").Append(SubmissionStatusJsonConverter.GetToken(request.Status.Value));
			}

			if (request.Slug.HasValue()) {
				endpoint.Append("&slug=").Append(Uri.EscapeDataString(request.Slug));
			}

			if (request.IsArchived.HasValue) {
				//	Lowercase literals: the API silently ignores archived=True (#1).
				endpoint.Append(request.IsArchived.Value ? "&archived=true" : "&archived=false");
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
	/// List submissions response.
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
		/// The submissions.
		/// </summary>
		[JsonPropertyName("data")]
		public IList<Submission> Submissions { get; init; } = [];
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator() {
		RuleFor(r => r.Status).Must(s => s is null || SubmissionStatusJsonConverter.GetToken(s.Value) is not null).WithMessage("'{PropertyName}' must be a known submission status.");
		RuleFor(r => r.Take).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
	}
}