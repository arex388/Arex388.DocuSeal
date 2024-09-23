using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class EventTypeJsonConverter :
	JsonConverter<EventType> {
	public override EventType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"click_email" => EventType.OpenedEmail,
			"complete_form" => EventType.CompletedForm,
			"send_email" => EventType.SentEmail,
			"start_form" => EventType.StartedForm,
			"view_form" => EventType.ViewedForm,
			_ => EventType.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		EventType value,
		JsonSerializerOptions options) {
		var eventType = value switch {
			EventType.CompletedForm => "complete_form",
			EventType.OpenedEmail => "click_email",
			EventType.SentEmail => "sent_email",
			EventType.StartedForm => "start_form",
			EventType.ViewedForm => "view_form",
			_ => null
		};

		writer.WriteStringValue(eventType);
	}
}