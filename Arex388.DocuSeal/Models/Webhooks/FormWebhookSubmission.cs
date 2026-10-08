using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// The submission of a form webhook event's submitter.
/// </summary>
public sealed class FormWebhookSubmission {
	/// <summary>
	/// The submission's audit log URL.
	/// </summary>
	[JsonPropertyName("audit_log_url")]
	public Uri? AuditUrl { get; init; }

	/// <summary>
	/// The submission's combined document URL. The document combines the signed documents and the audit log.
	/// </summary>
	[JsonPropertyName("combined_document_url")]
	public Uri? CombinedDocumentUrl { get; init; }

	/// <summary>
	/// The submission's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime CreatedAtUtc { get; init; }

	/// <summary>
	/// The submission's id.
	/// </summary>
	public SubmissionId Id { get; init; }

	/// <summary>
	/// The submission's status.
	/// </summary>
	public SubmissionStatus Status { get; init; }

	/// <summary>
	/// The submission's URL.
	/// </summary>
	public Uri Url { get; init; } = null!;

	/// <summary>
	/// The submission's dynamic content variables. The object is free-form.
	/// </summary>
	public JsonObject? Variables { get; init; }
}