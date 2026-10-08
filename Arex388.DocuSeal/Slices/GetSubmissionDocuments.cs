using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.GetSubmissionDocuments;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint returns the documents of a submission. If the submission has been completed, the final signed documents are returned; otherwise the partially filled documents.
/// </summary>
public static class GetSubmissionDocuments {
	/// <summary>
	/// Get submission documents request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => MustMerge is true
			? $"submissions/{Id}/documents?merge=true"
			: $"submissions/{Id}/documents";

		/// <summary>
		/// The unique identifier of the submission.
		/// </summary>
		public required SubmissionId Id { get; init; }

		/// <summary>
		/// Set <see langword="true" /> to merge all of the documents into a single PDF. The API defaults to <see langword="false" />.
		/// </summary>
		public bool? MustMerge { get; init; }
	}

	/// <summary>
	/// Get submission documents response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The submission's documents, or the single merged document when <see cref="Request.MustMerge" /> is set.
		/// </summary>
		public IList<SubmissionDocument> Documents { get; init; } = [];

		[JsonInclude]
		internal string? Error { get; init; }

		/// <summary>
		/// The submission's id.
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