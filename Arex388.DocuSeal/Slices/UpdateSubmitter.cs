using FluentValidation;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.UpdateSubmitter;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint provides allows you to update submitter details, pre-fill or update field values and re-send emails.
/// </summary>
public static class UpdateSubmitter {
	/// <summary>
	/// Update submitter request.
	/// </summary>
	public sealed class Request {
		/// <summary>
		/// The email address of the submitter.
		/// </summary>
		public string? Email { get; init; }

		internal string Endpoint => $"submitters/{Id}";

		/// <summary>
		/// Your application-specific unique string key to identify this submitter within your app.
		/// </summary>
		[JsonPropertyName("external_id")]
		public string? ExternalId { get; init; }

		/// <summary>
		/// A list of configurations for template document form fields.
		/// </summary>
		public IList<RequestField>? Fields { get; init; }

		/// <summary>
		/// The unique identifier of the submitter.
		/// </summary>
		[JsonIgnore]
		public SubmitterId Id { get; init; }

		/// <summary>
		/// Pass `true` to mark submitter as completed and auto-signed via API.
		/// </summary>
		[JsonPropertyName("completed")]
		public bool? IsCompleted { get; init; }

		/// <summary>
		/// The message for the submitter.
		/// </summary>
		public RequestMessage? Message { get; init; }

		/// <summary>
		/// Additional submitter information, keyed by name.
		/// </summary>
		public IDictionary<string, object?>? Metadata { get; init; }

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
		/// Specify Reply-To address to use in the notification emails.
		/// </summary>
		[JsonPropertyName("reply_to")]
		public string? ReplyToEmail { get; init; }

		/// <summary>
		/// Set `true` to require email 2FA verification via a one-time code sent to the email address in order to access the documents.
		/// </summary>
		[JsonPropertyName("require_email_2fa")]
		public bool? RequireEmail2fa { get; init; }

		/// <summary>
		/// Set `true` to require phone 2FA verification via a one-time code sent to the phone number in order to access the documents.
		/// </summary>
		[JsonPropertyName("require_phone_2fa")]
		public bool? RequirePhone2fa { get; init; }

		/// <summary>
		/// Set `true` to re-send signature request emails.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool? ResendEmail { get; init; }

		/// <summary>
		/// Set `true` to re-send signature request via phone number SMS.
		/// </summary>
		[JsonPropertyName("send_sms")]
		public bool? ResendSms { get; init; }

		/// <summary>
		/// An object with pre-filled values for the submission. Use field names for keys of the object. A value can be a string, a number, a boolean, or a collection of them. For more configurations see `fields` param.
		/// </summary>
		public IDictionary<string, object?>? Values { get; init; }
	}

	/// <summary>
	/// Update submitter request message. At least one of <see cref="Body" /> or <see cref="Subject" /> must be set.
	/// </summary>
	public sealed class RequestMessage {
		/// <summary>
		/// Custom signature request email body. Can include the following variables: {{template.name}}, {{submitter.link}}, {{account.name}}.
		/// </summary>
		public string? Body { get; init; }

		/// <summary>
		/// Custom signature request email subject.
		/// </summary>
		public string? Subject { get; init; }
	}

	/// <summary>
	/// Update submitter request field.
	/// </summary>
	public sealed class RequestField {
		/// <summary>
		/// Default value of the field. Use base64 encoded file or a public URL to the image file to set default signature or image fields.
		/// </summary>
		/// <remarks>
		/// The API accepts a string, a number, a boolean, or a collection of them.
		/// </remarks>
		[JsonPropertyName("default_value")]
		public object? DefaultValue { get; init; }

		/// <summary>
		/// Set `true` to make it impossible for the submitter to edit predefined field value.
		/// </summary>
		[JsonPropertyName("readonly")]
		public bool? IsReadonly { get; init; }

		/// <summary>
		/// Set `true` to make the field required.
		/// </summary>
		[JsonPropertyName("required")]
		public bool? IsRequired { get; init; }

		/// <summary>
		/// Document template field name.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// The field's display preferences.
		/// </summary>
		public RequestFieldPreferences? Preferences { get; init; }

		/// <summary>
		/// The field's validation rules.
		/// </summary>
		public RequestFieldValidation? Validation { get; init; }
	}

	/// <summary>
	/// Update submitter response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The updated submitter, including its embed source.
		/// </summary>
		public Submitter? Submitter { get; init; }
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator(
		IValidator<RequestMessage> requestMessageValidator,
		IValidator<RequestField> requestFieldValidator) {
		RuleFor(r => r.Fields!).ForEach(r => r.SetValidator(requestFieldValidator)).When(r => r.Fields is not null);
		RuleFor(r => r.Message).SetValidator(requestMessageValidator!);
		RuleFor(r => r.ReplyToEmail).EmailAddress().NotEmpty().When(r => r.ReplyToEmail.HasValue());
	}
}

file sealed class RequestMessageValidator :
	AbstractValidator<RequestMessage> {
	public RequestMessageValidator() {
		RuleFor(r => r.Subject).NotEmpty().When(r => !r.Body.HasValue()).WithMessage("'Body' or 'Subject' must be set.");
	}
}

file sealed class RequestFieldValidator :
	AbstractValidator<RequestField> {
	public RequestFieldValidator(
		IValidator<RequestFieldPreferences> requestFieldPreferencesValidator,
		IValidator<RequestFieldValidation> requestFieldValidationValidator) {
		RuleFor(r => r.Name).NotEmpty();
		RuleFor(r => r.Preferences).SetValidator(requestFieldPreferencesValidator!);
		RuleFor(r => r.Validation).SetValidator(requestFieldValidationValidator!);
	}
}