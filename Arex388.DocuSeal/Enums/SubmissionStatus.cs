namespace Arex388.DocuSeal;

/// <summary>
/// A submission status.
/// </summary>
public enum SubmissionStatus {
	/// <summary>
	/// An unknown submission's status.
	/// </summary>
	Unknown,

	/// <summary>
	/// A completed submission's status.
	/// </summary>
	Completed,

	/// <summary>
	/// A declined submission's status.
	/// </summary>
	Declined,

	/// <summary>
	/// An expired submission's status.
	/// </summary>
	Expired,

	/// <summary>
	/// A pending submission's status.
	/// </summary>
	Pending
}