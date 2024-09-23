using FluentValidation;
using static Arex388.DocuSeal.GetSubmitter;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides the functionality to retrieve information about a submitter.
/// </summary>
public static class GetSubmitter {
	/// <summary>
	/// Get submitter request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => $"submitters/{Id}";

		/// <summary>
		/// The unique identifier of the submitter.
		/// </summary>
		public required SubmitterId Id { get; init; }
	}

	/// <summary>
	/// Get submitter response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The submitter.
		/// </summary>
		public Submitter? Submitter { get; init; }
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