namespace Arex388.DocuSeal;

/// <summary>
/// An event type.
/// </summary>
public enum EventType {
	/// <summary>
	/// The form was completed via the API.
	/// </summary>
	ApiCompletedForm,

	/// <summary>
	/// An email bounced.
	/// </summary>
	BouncedEmail,

	/// <summary>
	/// A link in an email was clicked.
	/// </summary>
	ClickedEmail,

	/// <summary>
	/// A link in an SMS was clicked.
	/// </summary>
	ClickedSms,

	/// <summary>
	/// A spam complaint was received for an email.
	/// </summary>
	ComplaintEmail,

	/// <summary>
	/// The form was completed.
	/// </summary>
	CompletedForm,

	/// <summary>
	/// The verification was completed.
	/// </summary>
	CompletedVerification,

	/// <summary>
	/// The form was declined.
	/// </summary>
	DeclinedForm,

	/// <summary>
	/// A party was invited.
	/// </summary>
	InvitedParty,

	/// <summary>
	/// The email was opened.
	/// </summary>
	OpenedEmail,

	/// <summary>
	/// The form was sent via email.
	/// </summary>
	SentEmail,

	/// <summary>
	/// A reminder email was sent.
	/// </summary>
	SentReminderEmail,

	/// <summary>
	/// The form was sent via SMS.
	/// </summary>
	SentSms,

	/// <summary>
	/// A two-factor authentication SMS was sent.
	/// </summary>
	SentTwoFactorSms,

	/// <summary>
	/// The form was started.
	/// </summary>
	StartedForm,

	/// <summary>
	/// The verification was started.
	/// </summary>
	StartedVerification,

	/// <summary>
	/// An unknown event occurred.
	/// </summary>
	Unknown,

	/// <summary>
	/// The phone number was verified.
	/// </summary>
	VerifiedPhone,

	/// <summary>
	/// The form was viewed.
	/// </summary>
	ViewedForm
}