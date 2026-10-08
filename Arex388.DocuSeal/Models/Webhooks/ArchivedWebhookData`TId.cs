using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// The data of an archived webhook event.
/// </summary>
/// <typeparam name="TId">The archived object's id type: <see cref="SubmissionId"/> or <see cref="TemplateId"/>.</typeparam>
public sealed class ArchivedWebhookData<TId>
	where TId : struct {
	/// <summary>
	/// The archived object's archived timestamp.
	/// </summary>
	[JsonPropertyName("archived_at")]
	public DateTime? ArchivedAtUtc { get; init; }

	/// <summary>
	/// The archived object's id.
	/// </summary>
	public TId Id { get; init; }
}