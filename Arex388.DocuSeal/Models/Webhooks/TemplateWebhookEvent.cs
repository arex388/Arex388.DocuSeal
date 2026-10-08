namespace Arex388.DocuSeal;

/// <summary>
/// A template webhook event: <see cref="WebhookEventType.TemplateCreated"/> or <see cref="WebhookEventType.TemplateUpdated"/>.
/// </summary>
public sealed class TemplateWebhookEvent :
	WebhookEvent {
	/// <summary>
	/// The template webhook event's data. It is the template, in the same shape <see cref="IDocuSealClient.GetTemplateAsync(TemplateId, CancellationToken)"/> returns.
	/// </summary>
	public Template Data { get; init; } = null!;
}