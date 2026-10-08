using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CreateSubmissionFromEmails;

namespace Arex388.DocuSeal;

/// <summary>
/// This API endpoint allows you to create submissions for a document template and send them to the specified email addresses, one submission per address. This is a simplified version of the POST /submissions API to be used with Zapier or other automation tools.
/// </summary>
public static class CreateSubmissionFromEmails {
	/// <summary>
	/// Create submissions from emails request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The email addresses to send a submission to, one submission per address. An address cannot contain a comma.
		/// </summary>
		[JsonIgnore]
		public required IList<string> Emails { get; init; }

		//	The API takes the addresses as one comma-separated string.
		[JsonInclude, JsonPropertyName("emails")]
		internal string EmailsJoined => string.Join(",", Emails);

		internal string Endpoint { get; } = "submissions/emails";

		/// <summary>
		/// The message for the submissions. Unset is omitted from the request body; a message with neither member set is sent as <c>"message": {}</c>; the spec requires neither member.
		/// </summary>
		public RequestMessage? Message { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending. The API defaults to `true`.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool? MustEmail { get; init; }

		/// <summary>
		/// The unique identifier of the template.
		/// </summary>
		[JsonPropertyName("template_id")]
		public required TemplateId TemplateId { get; init; }
	}

	/// <summary>
	/// Create submissions from emails request message. Both members are optional; an unset member is omitted, so a message with neither set is sent as <c>{}</c>.
	/// </summary>
	public sealed class RequestMessage {
		/// <summary>
		/// Custom signature request email body. Can include the following variables: {{template.name}}, {{submission.name}}, {{submitter.link}}, {{account.name}}.
		/// </summary>
		public string? Body { get; init; }

		/// <summary>
		/// Custom signature request email subject.
		/// </summary>
		public string? Subject { get; init; }
	}

	/// <summary>
	/// Create submissions from emails response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The created submissions' submitters, one per email address in the request. Each one carries the <see cref="Submitter.SubmissionId" /> of its own submission.
		/// </summary>
		public IList<Submitter> Submitters { get; init; } = [];
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator() {
		RuleFor(r => r.Emails).NotEmpty();
		RuleForEach(r => r.Emails).NotEmpty().EmailAddress().Must(e => e is null || e.IndexOf(',') < 0).WithMessage("'Emails' must not contain a comma within an address.");
		RuleFor(r => r.TemplateId).NotEmpty();
	}
}