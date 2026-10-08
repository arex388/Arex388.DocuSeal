using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// The data of a form webhook event. It is the submitter the event happened to, with its submission and template.
/// </summary>
public sealed class FormWebhookData {
	/// <summary>
	/// The submitter's audit log URL.
	/// </summary>
	[JsonPropertyName("audit_log_url")]
	public Uri? AuditUrl { get; init; }

	/// <summary>
	/// The submitter's completed timestamp.
	/// </summary>
	[JsonPropertyName("completed_at")]
	public DateTime? CompletedAtUtc { get; init; }

	/// <summary>
	/// The submitter's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime CreatedAtUtc { get; init; }

	/// <summary>
	/// The submitter's decline reason.
	/// </summary>
	[JsonPropertyName("decline_reason")]
	public string? DeclineReason { get; init; }

	/// <summary>
	/// The submitter's declined timestamp.
	/// </summary>
	[JsonPropertyName("declined_at")]
	public DateTime? DeclinedAtUtc { get; init; }

	/// <summary>
	/// The submitter's completed documents.
	/// </summary>
	public IList<SubmissionDocument> Documents { get; init; } = [];

	/// <summary>
	/// The submitter's email.
	/// </summary>
	public string? Email { get; init; }

	/// <summary>
	/// The submitter's external id.
	/// </summary>
	[JsonPropertyName("external_id")]
	public string? ExternalId { get; init; }

	/// <summary>
	/// The submitter's id.
	/// </summary>
	public SubmitterId Id { get; init; }

	/// <summary>
	/// The submitter's IP address.
	/// </summary>
	[JsonPropertyName("ip")]
	public string? IpAddress { get; init; }

	/// <summary>
	/// The submitter's metadata. The object is free-form.
	/// </summary>
	public JsonObject? Metadata { get; init; }

	/// <summary>
	/// The submitter's name.
	/// </summary>
	public string? Name { get; init; }

	/// <summary>
	/// The submitter's opened timestamp.
	/// </summary>
	[JsonPropertyName("opened_at")]
	public DateTime? OpenedAtUtc { get; init; }

	/// <summary>
	/// The submitter's phone.
	/// </summary>
	public string? Phone { get; init; }

	/// <summary>
	/// The submitter's preferences. The object is free-form.
	/// </summary>
	public JsonObject? Preferences { get; init; }

	/// <summary>
	/// The submitter's role.
	/// </summary>
	public string Role { get; init; } = null!;

	/// <summary>
	/// The submitter's sent timestamp.
	/// </summary>
	[JsonPropertyName("sent_at")]
	public DateTime? SentAtUtc { get; init; }

	/// <summary>
	/// The submitter's status.
	/// </summary>
	public SubmitterStatus Status { get; init; }

	/// <summary>
	/// The submitter's submission.
	/// </summary>
	public FormWebhookSubmission? Submission { get; init; }

	/// <summary>
	/// The submitter's submission id.
	/// </summary>
	[JsonPropertyName("submission_id")]
	public SubmissionId SubmissionId { get; init; }

	/// <summary>
	/// The submitter's submission URL.
	/// </summary>
	[JsonPropertyName("submission_url")]
	public Uri? SubmissionUrl { get; init; }

	/// <summary>
	/// The submitter's template.
	/// </summary>
	public TemplateSummary? Template { get; init; }

	/// <summary>
	/// The submitter's updated timestamp.
	/// </summary>
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAtUtc { get; init; }

	/// <summary>
	/// The submitter's browser user agent.
	/// </summary>
	[JsonPropertyName("ua")]
	public string? UserAgent { get; init; }

	/// <summary>
	/// The submitter's field values.
	/// </summary>
	public IList<FieldValue> Values { get; init; } = [];
}