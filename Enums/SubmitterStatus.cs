namespace Arex388.DocuSeal.Models;

/// <summary>
/// A submitter status.
/// </summary>
public enum SubmitterStatus {
	/// <summary>
	/// A completed submitter's status.
	/// </summary>
	Completed,

	/// <summary>
	/// An opened submitter's status.
	/// </summary>
	Opened,

	/// <summary>
	/// A pending submitter's status.
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