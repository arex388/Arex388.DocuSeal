using Arex388.DocuSeal.Models;
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
		/// Filter submitters on name, email or phone partial match.
		/// </summary>
		public string? Search { get; init; }

		/// <summary>
		/// The number of submitters to return. Default value is 10. Maximum value is 100.
		/// </summary>
		public int Take { get; init; } = 10;

		/// <summary>
		/// The submission ID allows you to receive only the submitters related to that specific submission.
		/// </summary>
		public SubmissionId? SubmissionId { get; init; }

		//	========================================================================
		//	Utilities
		//	========================================================================

		private static string GetEndpoint(
			Request request) {
			var parameters = new HashSet<string> {
				$"limit={request.Take}"
			};

			if (request.Search.HasValue()) {
				parameters.Add($"q={request.Search}");
			}

			if (request.SubmissionId.HasValue) {
				parameters.Add($"submission_id={request.SubmissionId}");
			}

			return $"submitters?{parameters.StringJoin("&")}";
		}
	}

	/// <summary>
	/// List submitters response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
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