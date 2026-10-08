using FluentValidation;
using System.Globalization;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.ListSubmitters;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the ability to retrieve a list of submitters.
/// </summary>
public static class ListSubmitters {
	/// <summary>
	/// List submitters request.
	/// </summary>
	public sealed class Request {
		internal static readonly Request Instance = new();

		internal string Endpoint => GetEndpoint(this);

		/// <summary>
		/// Get only submitters with an id greater than this one. Pass the id from <see cref="ResponsePagination.Next" /> to load the next page.
		/// </summary>
		public SubmitterId? After { get; init; }

		/// <summary>
		/// Get only submitters with an id less than this one. Pass the id from <see cref="ResponsePagination.Previous" /> to load the previous page.
		/// </summary>
		public SubmitterId? Before { get; init; }

		/// <summary>
		/// Get only submitters that completed the submission after this date and time, in UTC. A value of <see cref="DateTimeKind.Local" /> kind is converted to UTC; a value of <see cref="DateTimeKind.Unspecified" /> kind is taken to already be UTC.
		/// </summary>
		public DateTime? CompletedAfterUtc { get; init; }

		/// <summary>
		/// Get only submitters that completed the submission before this date and time, in UTC. A value of <see cref="DateTimeKind.Local" /> kind is converted to UTC; a value of <see cref="DateTimeKind.Unspecified" /> kind is taken to already be UTC.
		/// </summary>
		public DateTime? CompletedBeforeUtc { get; init; }

		/// <summary>
		/// Filter submitters by the application-specific identifier provided for the submitter when the signature request was created.
		/// </summary>
		public string? ExternalId { get; init; }

		/// <summary>
		/// Filter submitters on name, email or phone partial match.
		/// </summary>
		public string? Search { get; init; }

		/// <summary>
		/// Filter submitters by unique slug.
		/// </summary>
		public string? Slug { get; init; }

		/// <summary>
		/// The submission ID allows you to receive only the submitters related to that specific submission.
		/// </summary>
		public SubmissionId? SubmissionId { get; init; }

		/// <summary>
		/// The number of submitters to return. Default value is 10. Maximum value is 100.
		/// </summary>
		public int Take { get; init; } = 10;

		//	========================================================================
		//	Utilities
		//	========================================================================

		private static string GetEndpoint(
			Request request) {
			//	The parameters are appended in a fixed order to a cached builder, with no intermediate set, list or join.
			var endpoint = StringBuilderCache.Acquire().Append("submitters?limit=").Append(request.Take);

			if (request.Search.HasValue()) {
				endpoint.Append("&q=").Append(Uri.EscapeDataString(request.Search));
			}

			if (request.SubmissionId.HasValue) {
				endpoint.Append("&submission_id=").Append(request.SubmissionId.Value.Value.ToString(CultureInfo.InvariantCulture));
			}

			if (request.Slug.HasValue()) {
				endpoint.Append("&slug=").Append(Uri.EscapeDataString(request.Slug));
			}

			if (request.CompletedAfterUtc.HasValue) {
				endpoint.Append("&completed_after=").Append(Uri.EscapeDataString(request.CompletedAfterUtc.Value.ToIso8601String()));
			}

			if (request.CompletedBeforeUtc.HasValue) {
				endpoint.Append("&completed_before=").Append(Uri.EscapeDataString(request.CompletedBeforeUtc.Value.ToIso8601String()));
			}

			if (request.ExternalId.HasValue()) {
				endpoint.Append("&external_id=").Append(Uri.EscapeDataString(request.ExternalId));
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
	/// List submitters response.
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
		/// The submitters.
		/// </summary>
		[JsonPropertyName("data")]
		public IList<Submitter> Submitters { get; init; } = [];
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