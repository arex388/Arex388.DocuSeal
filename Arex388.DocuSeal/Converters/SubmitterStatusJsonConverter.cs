using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmitterStatusJsonConverter :
	JsonConverter<SubmitterStatus> {
	public override SubmitterStatus Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(SubmitterStatus.Unknown);
		}

		return reader.ValueTextEquals("awaiting"u8) ? SubmitterStatus.Awaiting
			: reader.ValueTextEquals("completed"u8) ? SubmitterStatus.Completed
			: reader.ValueTextEquals("declined"u8) ? SubmitterStatus.Declined
			: reader.ValueTextEquals("opened"u8) ? SubmitterStatus.Opened
			: reader.ValueTextEquals("pending"u8) ? SubmitterStatus.Pending
			: reader.ValueTextEquals("sent"u8) ? SubmitterStatus.Sent
			: reader.Unmatched(SubmitterStatus.Unknown);
	}

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