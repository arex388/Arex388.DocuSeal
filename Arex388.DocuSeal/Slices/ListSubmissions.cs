using FluentValidation;
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
		/// Filter submissions by template folder name.
		/// </summary>
		public string? Folder { get; init; }

		/// <summary>
		/// Filter submissions based on submitters name, email or phone partial match.
		/// </summary>
		public string? Search { get; init; }

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
			var parameters = new HashSet<string> {
				$"limit={request.Take}"
			};

			if (request.Folder.HasValue()) {
				parameters.Add($"template_folder={request.Folder}");
			}

			if (request.Search.HasValue()) {
				parameters.Add($"q={request.Search}");
			}

			if (request.TemplateId.HasValue) {
				parameters.Add($"template_id={request.TemplateId}");
			}

			return $"submissions?{parameters.StringJoin("&")}";
		}
	}

	/// <summary>
	/// List submissions response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
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
		RuleFor(r => r.Take).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
	}
}