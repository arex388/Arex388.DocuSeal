using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.ArchiveSubmission;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint allows you to archive a submission.
/// </summary>
public static class ArchiveSubmission {
	/// <summary>
	/// Archive submission request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => $"submissions/{Id}";

		/// <summary>
		/// The unique identifier of the submission.
		/// </summary>
		public required SubmissionId Id { get; init; }
	}

	/// <summary>
	/// Archive submission response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The archived submission's archived timestamp.
		/// </summary>
		[JsonPropertyName("archived_at")]
		public DateTime? ArchivedAtUtc { get; init; }

		[JsonInclude]
		internal string? Error { get; init; }

		/// <summary>
		/// The archived submission's id.
		/// </summary>
		public SubmissionId? Id { get; init; }
	}
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