namespace Arex388.DocuSeal;

/// <summary>
/// A <see cref="WebhookEventType.TemplateArchived"/> webhook event.
/// </summary>
public sealed class TemplateArchivedWebhookEvent :
	WebhookEvent {
	/// <summary>
	/// The template archived webhook event's data. It is the archived template's id and archived timestamp.
	/// </summary>
	public ArchivedWebhookData<TemplateId> Data { get; init; } = null!;
}