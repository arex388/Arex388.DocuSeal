using FluentValidation;
using System.Globalization;
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
		internal string Endpoint { get; } = "submissions";

		//	The API takes expire_at as a string such as "2024-09-01 12:00:00 UTC", not as an ISO 8601 date-time.
		[JsonInclude, JsonPropertyName("expire_at")]
		internal string? ExpireAt => ExpireAtUtc?.AsUtc().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

		/// <summary>
		/// The date and time, in UTC, after which the submission becomes unavailable for signature. A value of <see cref="DateTimeKind.Local" /> kind is converted to UTC; a value of <see cref="DateTimeKind.Unspecified" /> kind is taken to already be UTC.
		/// </summary>
		[JsonIgnore]
		public DateTime? ExpireAtUtc { get; init; }

		/// <summary>
		/// The message for the submission. Unset is omitted from the request body; a message with neither member set is sent as <c>"message": {}</c>; the spec requires neither member.
		/// </summary>
		public RequestMessage? Message { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool? MustEmail { get; init; }

		/// <summary>
		/// Set `true` to send signature request via phone number and SMS.
		/// </summary>
		[JsonPropertyName("send_sms")]
		public bool? MustSms { get; init; }

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
		public SubmitterOrder? Order { get; init; }

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

		/// <summary>
		/// The dynamic content variables for a dynamic template document, keyed by variable name. A value can be a string, a number, a boolean, a collection, an object, or HTML content used to generate styled text, paragraphs, and tables.
		/// </summary>
		public IDictionary<string, object?>? Variables { get; init; }
	}

	/// <summary>
	/// Create submission request message. Both members are optional; an unset member is omitted, so a message with neither set is sent as <c>{}</c>.
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
	/// Create submission request submitter. At least one of <see cref="Email" />, <see cref="Phone" />, or <see cref="Name" /> must be set.
	/// </summary>
	public sealed class RequestSubmitter {
		/// <summary>
		/// The email address of the submitter.
		/// </summary>
		public string? Email { get; init; }

		/// <summary>
		/// Your application-specific unique string key to identify this submitter within your app.
		/// </summary>
		[JsonPropertyName("external_id")]
		public string? ExternalId { get; init; }

		/// <summary>
		/// A list of configurations for template document form fields.
		/// </summary>
		public IList<RequestSubmitterField>? Fields { get; init; }

		/// <summary>
		/// The role name of the previous party that should invite this submitter via email.
		/// </summary>
		[JsonPropertyName("invite_by")]
		public string? InviteBy { get; init; }

		/// <summary>
		/// Pass `true` to mark submitter as completed and auto-signed via API.
		/// </summary>
		[JsonPropertyName("completed")]
		public bool? IsCompleted { get; init; }

		/// <summary>
		/// The custom signature request email message for this submitter. Unset is omitted from the request body; a message with neither member set is sent as <c>"message": {}</c>; the spec requires neither member.
		/// </summary>
		public RequestMessage? Message { get; init; }

		/// <summary>
		/// Additional submitter information, keyed by name.
		/// </summary>
		public IDictionary<string, object?>? Metadata { get; init; }

		/// <summary>
		/// Set `false` to disable signature request emails sending.
		/// </summary>
		[JsonPropertyName("send_email")]
		public bool? MustEmail { get; init; }

		/// <summary>
		/// Set `true` to send signature request via phone number and SMS.
		/// </summary>
		[JsonPropertyName("send_sms")]
		public bool? MustSms { get; init; }

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
		/// The submitter's position in the signing workflow (e.g., 0 for the first signer, 1 for the second). Submitters with the same number form an order group. By default, submitters are ordered as in <see cref="Request.Submitters" />.
		/// </summary>
		/// <remarks>
		/// Serialized as the API's per-submitter <c>order</c>, which is unrelated to the request-level <see cref="Request.Order" />.
		/// </remarks>
		[JsonPropertyName("order")]
		public int? OrderGroup { get; init; }

		/// <summary>
		/// The phone number of the submitter, formatted according to the E.164 standard.
		/// </summary>
		public string? Phone { get; init; }

		/// <summary>
		/// Specify Reply-To address to use in the notification emails for this submitter.
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
		/// The role name or title of the submitter.
		/// </summary>
		public string? Role { get; init; }

		/// <summary>
		/// The role names to merge into this one submitter.
		/// </summary>
		public IList<string>? Roles { get; init; }

		/// <summary>
		/// An object with pre-filled values for the submission. Use field names for keys of the object. A value can be a string, a number, a boolean, or a collection of them. For more configurations see `fields` param.
		/// </summary>
		public IDictionary<string, object?>? Values { get; init; }
	}

	/// <summary>
	/// Create submission request submitter field.
	/// </summary>
	public sealed class RequestSubmitterField {
		/// <summary>
		/// Default value of the field. Use base64 encoded file or a public URL to the image file to set default signature or image fields.
		/// </summary>
		/// <remarks>
		/// The API accepts a string, a number, a boolean, or a collection of them.
		/// </remarks>
		[JsonPropertyName("default_value")]
		public object? DefaultValue { get; init; }

		/// <summary>
		/// Field description displayed on the signing form. Supports Markdown.
		/// </summary>
		public string? Description { get; init; }

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
		/// Field title displayed on the signing form instead of the name. Supports Markdown.
		/// </summary>
		public string? Title { get; init; }

		/// <summary>
		/// The field's validation rules.
		/// </summary>
		public RequestFieldValidation? Validation { get; init; }
	}

	/// <summary>
	/// Create submission response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The id of the created submission. Every submitter in <see cref="Submitters"/> belongs to it; call <c>GetSubmissionAsync</c> with it to get the submission itself.
		/// </summary>
		public SubmissionId? SubmissionId { get; init; }

		/// <summary>
		/// The created submission's submitters, one per submitter in the request.
		/// </summary>
		public IList<Submitter> Submitters { get; init; } = [];
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator(
		IValidator<RequestSubmitter> requestSubmitterValidator) {
		RuleFor(r => r.OnCompletedBccEmail).EmailAddress().When(r => r.OnCompletedBccEmail.HasValue());
		RuleFor(r => r.ReplyToEmail).EmailAddress().NotEmpty().When(r => r.ReplyToEmail.HasValue());
		RuleFor(r => r.Submitters).ForEach(r => r.SetValidator(requestSubmitterValidator)).NotEmpty();
		RuleFor(r => r.TemplateId).NotEmpty();
	}
}

file sealed class RequestSubmitterValidator :
	AbstractValidator<RequestSubmitter> {
	public RequestSubmitterValidator(
		IValidator<RequestSubmitterField> requestSubmitterFieldValidator) {
		RuleFor(r => r.Email).EmailAddress().When(r => r.Email.HasValue());
		RuleFor(r => r.Name).NotEmpty().When(r => !r.Email.HasValue() && !r.Phone.HasValue()).WithMessage("'Email', 'Phone' or 'Name' must be set.");
		RuleFor(r => r.Fields!).ForEach(r => r.SetValidator(requestSubmitterFieldValidator)).When(r => r.Fields is not null);
		RuleFor(r => r.OrderGroup).GreaterThanOrEqualTo(0).When(r => r.OrderGroup.HasValue);
		RuleFor(r => r.ReplyToEmail).EmailAddress().When(r => r.ReplyToEmail.HasValue());
	}
}

file sealed class RequestSubmitterFieldValidator :
	AbstractValidator<RequestSubmitterField> {
	public RequestSubmitterFieldValidator(
		IValidator<RequestFieldPreferences> requestFieldPreferencesValidator,
		IValidator<RequestFieldValidation> requestFieldValidationValidator) {
		RuleFor(r => r.Name).NotEmpty();
		RuleFor(r => r.Preferences).SetValidator(requestFieldPreferencesValidator!);
		RuleFor(r => r.Validation).SetValidator(requestFieldValidationValidator!);
	}
}