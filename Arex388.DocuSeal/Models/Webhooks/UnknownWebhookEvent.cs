using System.Text.Json;

namespace Arex388.DocuSeal;

/// <summary>
/// A webhook event whose <c>event_type</c> the client does not know. Its <see cref="WebhookEvent.Type"/> is <see cref="WebhookEventType.Unknown"/>.
/// </summary>
public sealed class UnknownWebhookEvent :
	WebhookEvent {
	/// <summary>
	/// The unknown webhook event's data, unparsed.
	/// </summary>
	public JsonElement Data { get; init; }

	/// <summary>
	/// The unknown webhook event's <c>event_type</c>, as the payload carried it.
	/// </summary>
	public string RawType { get; init; } = null!;
}