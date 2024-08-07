using Arex388.DocuSeal.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class EventTypeJsonConverter :
	JsonConverter<EventType> {
	/// <inheritdoc />
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

	/// <inheritdoc />
	public override void Write(
		Utf8JsonWriter writer,
		EventType value,
		JsonSerializerOptions options) => throw new NotImplementedException();
}