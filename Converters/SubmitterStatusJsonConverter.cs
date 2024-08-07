using Arex388.DocuSeal.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmitterStatusJsonConverter :
	JsonConverter<SubmitterStatus> {
	/// <inheritdoc />
	public override SubmitterStatus Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"completed" => SubmitterStatus.Completed,
			"opened" => SubmitterStatus.Opened,
			"pending" => SubmitterStatus.Pending,
			"sent" => SubmitterStatus.Sent,
			_ => SubmitterStatus.Unknown
		};

	/// <inheritdoc />
	public override void Write(
		Utf8JsonWriter writer,
		SubmitterStatus value,
		JsonSerializerOptions options) {
		var status = value switch {
			SubmitterStatus.Completed => "completed",
			SubmitterStatus.Opened => "opened",
			SubmitterStatus.Pending => "pending",
			SubmitterStatus.Sent => "sent",
			_ => null
		};

		writer.WriteStringValue(status);
	}
}