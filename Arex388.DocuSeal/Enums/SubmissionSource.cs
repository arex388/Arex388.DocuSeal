namespace Arex388.DocuSeal;

/// <summary>
/// A submission source.
/// </summary>
public enum SubmissionSource {
	/// <summary>
	/// An unknown submission source.
	/// </summary>
	Unknown,

	/// <summary>
	/// Created via the API.
	/// </summary>
	Api,

	/// <summary>
	/// Created via a bulk send.
	/// </summary>
	Bulk,

	/// <summary>
	/// Created via an embedded form.
	/// </summary>
	Embed,

	/// <summary>
	/// Created via an invitation.
	/// </summary>
	Invite,

	/// <summary>
	/// Created via a link.
	/// </summary>
	Link,

	/// <summary>
	/// Created via MCP.
	/// </summary>
	Mcp,

	/// <summary>
	/// Created by the submitter.
	/// </summary>
	Self
}