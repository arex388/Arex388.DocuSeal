using FluentValidation;
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
		/// Filter templates by folder name.
		/// </summary>
		public string? Folder { get; init; }

		/// <summary>
		/// Get only archived templates instead of active ones.
		/// </summary>
		public bool IsArchived { get; init; }

		/// <summary>
		/// Filter templates based on the name partial match.
		/// </summary>
		public string? Search { get; init; }

		/// <summary>
		/// The number of templates to return. Default value is 10. Maximum value is 100.
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

			if (request.Folder.HasValue()) {
				parameters.Add($"folder={request.Folder}");
			}

			if (request.IsArchived) {
				parameters.Add("archived=True");
			}

			if (request.Search.HasValue()) {
				parameters.Add($"q={request.Search}");
			}

			return $"templates?{parameters.StringJoin("&")}";
		}
	}

	/// <summary>
	/// List templates response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
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