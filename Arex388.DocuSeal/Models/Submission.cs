using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A submission.
/// </summary>
public sealed class Submission {
	/// <summary>
	/// The submission's archived timestamp.
	/// </summary>
	[JsonPropertyName("archived_at")]
	public DateTime? ArchivedAtUtc { get; init; }

	/// <summary>
	/// The submission's audit log URL.
	/// </summary>
	[JsonPropertyName("audit_log_url")]
	public Uri AuditUrl { get; init; } = null!;

	/// <summary>
	/// The submission's completed timestamp.
	/// </summary>
	[JsonPropertyName("completed_at")]
	public DateTime? CompletedAtUtc { get; init; }

	/// <summary>
	/// The submission's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime CreatedAtUtc { get; init; }

	/// <summary>
	/// The submission's creation user.
	/// </summary>
	[JsonPropertyName("created_by_user")]
	public User CreatedBy { get; init; } = null!;

	/// <summary>
	/// The submission's email.
	/// </summary>
	public string Email { get; init; } = null!;

	[JsonInclude]
	internal string? Error { get; init; }

	/// <summary>
	/// The submission's events.
	/// </summary>
	[JsonPropertyName("submission_events")]
	public IList<Event> Events { get; init; } = [];

	/// <summary>
	/// The submission's id.
	/// </summary>
	[JsonPropertyName("submission_id")]
	public SubmissionId Id { get; init; }

	/// <summary>
	/// The submission's name.
	/// </summary>
	public string? Name { get; init; }

	/// <summary>
	/// The submission's opened timestamp.
	/// </summary>
	[JsonPropertyName("opened_at")]
	public DateTime? OpenedAtUtc { get; init; }

	/// <summary>
	/// The submission's sent timestamp.
	/// </summary>
	[JsonPropertyName("sent_at")]
	public DateTime? SentAtUtc { get; init; }

	/// <summary>
	/// The submission's status.
	/// </summary>
	//[JsonConverter(typeof(SubmitterStatusJsonConverter))]
	public SubmitterStatus Status { get; init; }

	/// <summary>
	/// The submission's submitters.
	/// </summary>
	public IList<Submitter> Submitters { get; init; } = [];

	/// <summary>
	/// The submission's submitters order.
	/// </summary>
	[/*JsonConverter(typeof(SubmitterOrderJsonConverter)), */JsonPropertyName("submitters_order")]
	public SubmitterOrder SubmittersOrder { get; init; }

	/// <summary>
	/// The submission's template.
	/// </summary>
	public Template Template { get; init; } = null!;

	/// <summary>
	/// The submission's updated timestamp.
	/// </summary>
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAtUtc { get; init; }
}