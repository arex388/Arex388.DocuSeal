using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A webhook event. <see cref="DocuSealWebhook.Parse(string)"/> returns the subtype that matches the payload's <see cref="Type"/>, so pattern match on the subtype to reach its data.
/// </summary>
public abstract class WebhookEvent {
	private protected WebhookEvent() {
	}

	/// <summary>
	/// The webhook event's timestamp.
	/// </summary>
	[JsonPropertyName("timestamp")]
	public DateTime TimestampUtc { get; init; }

	/// <summary>
	/// The webhook event's type.
	/// </summary>
	[JsonPropertyName("event_type")]
	public WebhookEventType Type { get; init; }
}