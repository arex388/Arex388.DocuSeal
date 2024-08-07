//using FluentValidation;
//using System.Text.Json.Serialization;
//using static Arex388.DocuSeal.CreateSubmissionSimple;

//namespace Arex388.DocuSeal;

///// <summary>
///// This API endpoint allows you to create submissions for a document template and send them to the specified email addresses. This is a simplified version of the POST /submissions API to be used with Zapier or other automation tools.
///// </summary>
//public static class CreateSubmissionSimple {
//	/// <summary>
//	/// Create submission simple request.
//	/// </summary>
//	public sealed class Request {
//		/// <summary>
//		/// A comma-separated list of email addresses to send the submission to.
//		/// </summary>
//		[JsonIgnore]
//		public IList<string> Emails { get; init; } = [];

//		[JsonInclude, JsonPropertyName("emails")]
//		internal string EmailsConcatenated => Emails.StringJoin(",");

//		internal string Endpoint => "submissions/emails";

//		/// <summary>
//		/// The message for the submission.
//		/// </summary>
//		public RequestMessage? Message { get; init; }

//		/// <summary>
//		/// Set `false` to disable signature request emails sending.
//		/// </summary>
//		[JsonPropertyName("send_email")]
//		public bool MustEmail { get; init; } = true;

//		/// <summary>
//		/// The unique identifier of the template.
//		/// </summary>
//		[JsonPropertyName("template_id")]
//		public required TemplateId TemplateId { get; init; }
//	}

//	/// <summary>
//	/// Create submission simple request message.
//	/// </summary>
//	public sealed class RequestMessage {
//		/// <summary>
//		/// Custom signature request email body. Can include the following variables: {{template.name}}, {{submitter.link}}, {{account.name}}.
//		/// </summary>
//		public required string Body { get; init; }

//		/// <summary>
//		/// Custom signature request email subject.
//		/// </summary>
//		public required string Subject { get; init; }
//	}

//	/// <summary>
//	/// Create submission simple response.
//	/// </summary>
//	public sealed class Response :
//		ResponseBase<Response>;
//}

////	================================================================================
////	Validators
////	================================================================================

//file sealed class RequestValidator :
//	AbstractValidator<Request> {
//	public RequestValidator(
//		IValidator<RequestMessage> requestMessageValidator) {
//		RuleFor(r => r.Emails).ForEach(r => r.EmailAddress()).NotEmpty();
//		RuleFor(r => r.Message).SetValidator(requestMessageValidator!);
//		RuleFor(r => r.TemplateId).NotEmpty();
//	}
//}

//file sealed class RequestMessageValidator :
//	AbstractValidator<RequestMessage> {
//	public RequestMessageValidator() {
//		RuleFor(r => r.Body).NotEmpty();
//		RuleFor(r => r.Subject).NotEmpty();
//	}
//}