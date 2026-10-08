using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldAlignJsonConverter :
	JsonConverter<FieldAlign> {
	public override FieldAlign Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(FieldAlign.Unknown);
		}

		return reader.ValueTextEquals("center"u8) ? FieldAlign.Center
			: reader.ValueTextEquals("left"u8) ? FieldAlign.Left
			: reader.ValueTextEquals("right"u8) ? FieldAlign.Right
			: reader.Unmatched(FieldAlign.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		FieldAlign value,
		JsonSerializerOptions options) {
		var fieldAlign = value switch {
			FieldAlign.Center => "center",
			FieldAlign.Left => "left",
			FieldAlign.Right => "right",
			_ => null
		};

		writer.WriteStringValue(fieldAlign);
	}
}