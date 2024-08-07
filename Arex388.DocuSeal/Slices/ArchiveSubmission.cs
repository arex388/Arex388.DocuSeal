using FluentValidation;
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
		ResponseBase<Response>;
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