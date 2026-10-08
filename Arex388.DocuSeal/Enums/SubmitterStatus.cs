namespace Arex388.DocuSeal;

/// <summary>
/// A submitter status.
/// </summary>
public enum SubmitterStatus {
	/// <summary>
	/// An awaiting submitter's status.
	/// </summary>
	Awaiting,

	/// <summary>
	/// A completed submitter's status.
	/// </summary>
	Completed,

	/// <summary>
	/// A declined submitter's status.
	/// </summary>
	Declined,

	/// <summary>
	/// An opened submitter's status.
	/// </summary>
	Opened,

	/// <summary>
	/// A pending submitter's status. The OpenAPI spec does not list this status for submitters; it is kept for compatibility.
	/// </summary>
	Pending,

	/// <summary>
	/// A sent submitter's status.
	/// </summary>
	Sent,

	/// <summary>
	/// An unknown submitter's status.
	/// </summary>
	Unknown
}