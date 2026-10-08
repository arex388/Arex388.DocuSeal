using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmitterStatusJsonConverter :
	JsonConverter<SubmitterStatus> {
	public override SubmitterStatus Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"awaiting" => SubmitterStatus.Awaiting,
			"completed" => SubmitterStatus.Completed,
			"declined" => SubmitterStatus.Declined,
			"opened" => SubmitterStatus.Opened,
			"pending" => SubmitterStatus.Pending,
			"sent" => SubmitterStatus.Sent,
			_ => SubmitterStatus.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		SubmitterStatus value,
		JsonSerializerOptions options) {
		var submitterStatus = value switch {
			SubmitterStatus.Awaiting => "awaiting",
			SubmitterStatus.Completed => "completed",
			SubmitterStatus.Declined => "declined",
			SubmitterStatus.Opened => "opened",
			SubmitterStatus.Pending => "pending",
			SubmitterStatus.Sent => "sent",
			_ => null
		};

		writer.WriteStringValue(submitterStatus);
	}
}