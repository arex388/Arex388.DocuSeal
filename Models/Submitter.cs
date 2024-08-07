using Arex388.DocuSeal.Converters;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Models;

/// <summary>
/// A submitter.
/// </summary>
public sealed class Submitter :
	IErrable {
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
	/// The submitter's email.
	/// </summary>
	public string Email { get; init; } = null!;

	/// <inheritdoc />
	public string? Error { get; init; }

	/// <summary>
	/// The submitter's id.
	/// </summary>
	public SubmitterId Id { get; init; }

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
	[JsonConverter(typeof(SubmitterStatusJsonConverter))]
	public SubmitterStatus Status { get; init; }

	/// <summary>
	/// The submitter's submission id.
	/// </summary>
	[JsonPropertyName("submission_id")]
	public SubmissionId SubmissionId { get; init; }

	/// <summary>
	/// The submitter's updated timestamp.
	/// </summary>
	[JsonPropertyName("updated_at")]
	public DateTime? UpdatedAtUtc { get; init; }
}