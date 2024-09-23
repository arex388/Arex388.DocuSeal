namespace Arex388.DocuSeal;

/// <summary>
/// An event type.
/// </summary>
public enum EventType {
	/// <summary>
	/// The form was completed.
	/// </summary>
	CompletedForm,

	/// <summary>
	/// The form was opened via email.
	/// </summary>
	OpenedEmail,

	/// <summary>
	/// The form was sent via email.
	/// </summary>
	SentEmail,

	/// <summary>
	/// The form was started.
	/// </summary>
	StartedForm,

	/// <summary>
	/// An unknown event occurred.
	/// </summary>
	Unknown,

	/// <summary>
	/// The form was viewed.
	/// </summary>
	ViewedForm
}