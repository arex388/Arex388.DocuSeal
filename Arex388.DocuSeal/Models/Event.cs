using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// An event.
/// </summary>
public sealed class Event {
	/// <summary>
	/// The event's id.
	/// </summary>
	public EventId Id { get; init; }

	/// <summary>
	/// The event's occurance timestamp.
	/// </summary>
	[JsonPropertyName("event_timestamp")]
	public DateTime OccuredAtUtc { get; init; }

	/// <summary>
	/// The event's submitter id.
	/// </summary>
	[JsonPropertyName("submitter_id")]
	public SubmitterId SubmitterId { get; init; }

	/// <summary>
	/// The event's type.
	/// </summary>
	[/*JsonConverter(typeof(EventTypeJsonConverter)), */JsonPropertyName("event_type")]
	public EventType Type { get; init; }
}