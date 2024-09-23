using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.CreateSubmission;

namespace Arex388.DocuSeal;

/// <summary>
/// This API endpoint allows you to create signature requests (submissions) for a document template and send them to the specified submitters (signers).
/// </summary>
public static class CreateSubmission {
	/// <summary>
	/// Create submission request.
	/// </summary>
	public sealed class Request {
		internal string Endpoint => "submissions";

		/// <summary>
		/// The message for the submission.
		/// </summary>
		public RequestMessage? Message { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool MustEmail { get; init; } = true;

		/// <summary>
		/// Set `true` to send signature request via phone number and SMS.
		/// </summary>
		[JsonPropertyName("send_sms")]
		public bool MustSms { get; init; }

		/// <summary>
		/// Specify BCC address to send signed documents to after the completion.
		/// </summary>
		[JsonPropertyName("bcc_completed")]
		public string? OnCompletedBccEmail { get; init; }

		/// <summary>
		/// Specify URL to redirect to after the submission completion.
		/// </summary>
		[JsonPropertyName("completed_redirect_url")]
		public string? OnCompletedUrl { get; init; }

		/// <summary>
		/// Pass 'random' to send signature request emails to all parties right away. The order is 'preserved' by default so the second party will receive a signature request email only after the document is signed by the first party.
		/// </summary>
		//[JsonConverter(typeof(SubmitterOrderJsonConverter))]
		public SubmitterOrder Order { get; init; } = SubmitterOrder.Preserved;

		/// <summary>
		/// Specify Reply-To address to use in the notification emails.
		/// </summary>
		[JsonPropertyName("reply_to")]
		public string? ReplyToEmail { get; init; }

		/// <summary>
		/// The list of submitters for the submission.
		/// </summary>
		public required IList<RequestSubmitter> Submitters { get; init; } = [];

		/// <summary>
		/// The unique identifier of the template. Document template forms can be created via Web UI, PDF and DOCX API, or HTML API.
		/// </summary>
		[JsonPropertyName("template_id")]
		public required TemplateId TemplateId { get; init; }
	}

	/// <summary>
	/// Create submission request message.
	/// </summary>
	public sealed class RequestMessage {
		/// <summary>
		/// Custom signature request email body. Can include the following variables: {{template.name}}, {{submitter.link}}, {{account.name}}.
		/// </summary>
		public required string Body { get; init; }

		/// <summary>
		/// Custom signature request email subject.
		/// </summary>
		public required string Subject { get; init; }
	}

	/// <summary>
	/// Create submission request submitter.
	/// </summary>
	public sealed class RequestSubmitter {
		/// <summary>
		/// The email address of the submitter.
		/// </summary>
		public required string Email { get; init; }

		/// <summary>
		/// A list of configurations for template document form fields.
		/// </summary>
		public IList<RequestSubmitterField> Fields { get; init; } = [];

		/// <summary>
		/// Pass `true` to mark submitter as completed and auto-signed via API.
		/// </summary>
		[JsonPropertyName("completed")]
		public bool IsCompleted { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool MustEmail { get; init; } = true;

		/// <summary>
		/// Set `true` to send signature request via phone number and SMS.
		/// </summary>
		[JsonPropertyName("send_sms")]
		public bool MustSms { get; init; }

		/// <summary>
		/// The name of the submitter.
		/// </summary>
		public string? Name { get; init; }

		/// <summary>
		/// Submitter specific URL to redirect to after the submission completion.
		/// </summary>
		[JsonPropertyName("completed_redirect_url")]
		public string? OnCompletedUrl { get; init; }

		/// <summary>
		/// The phone number of the submitter, formatted according to the E.164 standard.
		/// </summary>
		public string? Phone { get; init; }

		/// <summary>
		/// The role name or title of the submitter.
		/// </summary>
		public string? Role { get; init; }

		/// <summary>
		/// An object with pre-filled values for the submission. Use field names for keys of the object. For more configurations see `fields` param.
		/// </summary>
		public IDictionary<string, string> Values { get; init; } = DictionaryHelper.Empty;
	}

	/// <summary>
	/// Create submission request submitter field.
	/// </summary>
	public sealed class RequestSubmitterField {
		/// <summary>
		/// Default value of the field. Use base64 encoded file or a public URL to the image file to set default signature or image fields.
		/// </summary>
		[JsonPropertyName("default_value")]
		public string? DefaultValue { get; init; }

		/// <summary>
		/// Set `true` to make it impossible for the submitter to edit predefined field value.
		/// </summary>
		[JsonPropertyName("readonly")]
		public bool IsReadonly { get; init; }

		/// <summary>
		/// Document template field name.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// A custom message to display on pattern validation failure.
		/// </summary>
		[JsonPropertyName("invalid_message")]
		public string? ValidationFailedMessage { get; init; }

		/// <summary>
		/// HTML field validation pattern string based on https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes/pattern specification.
		/// </summary>
		[JsonPropertyName("validation_pattern")]
		public string? ValidationPattern { get; init; }
	}

	/// <summary>
	/// Create submission response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The submission.
		/// </summary>
		public Submission? Submission { get; set; }
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator(
		IValidator<RequestMessage> requestMessageValidator,
		IValidator<RequestSubmitter> requestSubmitterValidator) {
		RuleFor(r => r.Message).SetValidator(requestMessageValidator!);
		RuleFor(r => r.OnCompletedBccEmail).EmailAddress().When(r => r.OnCompletedBccEmail.HasValue());
		RuleFor(r => r.ReplyToEmail).EmailAddress().NotEmpty().When(r => r.ReplyToEmail.HasValue());
		RuleFor(r => r.Submitters).ForEach(r => r.SetValidator(requestSubmitterValidator)).NotEmpty();
		RuleFor(r => r.TemplateId).NotEmpty();
	}
}

file sealed class RequestMessageValidator :
	AbstractValidator<RequestMessage> {
	public RequestMessageValidator() {
		RuleFor(r => r.Body).NotEmpty();
		RuleFor(r => r.Subject).NotEmpty();
	}
}

file sealed class RequestSubmitterValidator :
	AbstractValidator<RequestSubmitter> {
	public RequestSubmitterValidator(
		IValidator<RequestSubmitterField> requestSubmitterFieldValidator) {
		RuleFor(r => r.Email).EmailAddress().NotEmpty();
		RuleFor(r => r.Fields).ForEach(r => r.SetValidator(requestSubmitterFieldValidator));
	}
}

file sealed class RequestSubmitterFieldValidator :
	AbstractValidator<RequestSubmitterField> {
	public RequestSubmitterFieldValidator() {
		RuleFor(r => r.Name).NotEmpty();
	}
}