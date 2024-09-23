using FluentValidation;
using static Arex388.DocuSeal.GetSubmission;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to retrieve information about a submission.
/// </summary>
public static class GetSubmission {
	/// <summary>
	/// Get submission request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => $"submissions/{Id}";

		/// <summary>
		/// The unique identifier of the submission.
		/// </summary>
		public required SubmissionId Id { get; init; }
	}

	/// <summary>
	/// Get submission response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The submission.
		/// </summary>
		public Submission? Submission { get; init; }
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