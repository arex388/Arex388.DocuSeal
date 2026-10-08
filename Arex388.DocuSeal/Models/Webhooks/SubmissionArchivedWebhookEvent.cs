namespace Arex388.DocuSeal;

/// <summary>
/// A <see cref="WebhookEventType.SubmissionArchived"/> webhook event.
/// </summary>
public sealed class SubmissionArchivedWebhookEvent :
	WebhookEvent {
	/// <summary>
	/// The submission archived webhook event's data. It is the archived submission's id and archived timestamp.
	/// </summary>
	public ArchivedWebhookData<SubmissionId> Data { get; init; } = null!;
}