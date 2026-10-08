using FluentValidation;
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
			var parameters = new HashSet<string> {
				$"limit={request.Take}"
			};

			if (request.Search.HasValue()) {
				parameters.Add($"q={Uri.EscapeDataString(request.Search)}");
			}

			if (request.SubmissionId.HasValue) {
				parameters.Add($"submission_id={request.SubmissionId}");
			}

			if (request.Slug.HasValue()) {
				parameters.Add($"slug={Uri.EscapeDataString(request.Slug)}");
			}

			if (request.CompletedAfterUtc.HasValue) {
				parameters.Add($"completed_after={Uri.EscapeDataString(request.CompletedAfterUtc.Value.ToIso8601String())}");
			}

			if (request.CompletedBeforeUtc.HasValue) {
				parameters.Add($"completed_before={Uri.EscapeDataString(request.CompletedBeforeUtc.Value.ToIso8601String())}");
			}

			if (request.ExternalId.HasValue()) {
				parameters.Add($"external_id={Uri.EscapeDataString(request.ExternalId)}");
			}

			if (request.After.HasValue) {
				parameters.Add($"after={request.After}");
			}

			if (request.Before.HasValue) {
				parameters.Add($"before={request.Before}");
			}

			return $"submitters?{parameters.StringJoin("&")}";
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