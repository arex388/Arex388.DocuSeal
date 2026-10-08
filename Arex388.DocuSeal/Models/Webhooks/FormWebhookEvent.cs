namespace Arex388.DocuSeal;

/// <summary>
/// A form webhook event: <see cref="WebhookEventType.FormViewed"/>, <see cref="WebhookEventType.FormStarted"/>, <see cref="WebhookEventType.FormCompleted"/>, or <see cref="WebhookEventType.FormDeclined"/>.
/// </summary>
public sealed class FormWebhookEvent :
	WebhookEvent {
	/// <summary>
	/// The form webhook event's data. It is the submitter the event happened to.
	/// </summary>
	public FormWebhookData Data { get; init; } = null!;
}