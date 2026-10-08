using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A submitter.
/// </summary>
public sealed class Submitter {
	/// <summary>
	/// The submitter's completed timestamp.
	/// </summary>
	[JsonPropertyName("completed_at")]
	public DateTime? CompletedAtUtc { get; init; }

	/// <summary>
	/// The submitter's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime? CreatedAtUtc { get; init; }

	/// <summary>
	/// The submitter's declined timestamp.
	/// </summary>
	[JsonPropertyName("declined_at")]
	public DateTime? DeclinedAtUtc { get; init; }

	/// <summary>
	/// The submitter's completed or signed documents.
	/// </summary>
	public IList<SubmissionDocument> Documents { get; init; } = [];

	/// <summary>
	/// The submitter's email.
	/// </summary>
	public string? Email { get; init; }

	[JsonInclude]
	internal string? Error { get; init; }

	/// <summary>
	/// The submitter's events.
	/// </summary>
	[JsonPropertyName("submission_events")]
	public IList<Event> Events { get; init; } = [];

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
	/// The submitter's slug. It is the key of the submitter's signing link.
	/// </summary>
	public string Slug { get; init; } = null!;

	/// <summary>
	/// The submitter's status.
	/// </summary>
	//[JsonConverter(typeof(SubmitterStatusJsonConverter))]
	public SubmitterStatus Status { get; init; }

	/// <summary>
	/// The submitter's submission id.
	/// </summary>
	[JsonPropertyName("submission_id")]
	public SubmissionId SubmissionId { get; init; }

	/// <summary>
	/// The submitter's template.
	/// </summary>
	public SubmitterTemplate? Template { get; init; }

	/// <summary>
	/// The submitter's updated timestamp.
	/// </summary>
	[JsonPropertyName("updated_at")]
	public DateTime? UpdatedAtUtc { get; init; }

	/// <summary>
	/// The submitter's uuid.
	/// </summary>
	public Guid Uuid { get; init; }

	/// <summary>
	/// The submitter's field values.
	/// </summary>
	public IList<FieldValue> Values { get; init; } = [];
}