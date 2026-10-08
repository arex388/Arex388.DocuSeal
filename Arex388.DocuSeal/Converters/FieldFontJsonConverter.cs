using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldFontJsonConverter :
	JsonConverter<FieldFont> {
	public override FieldFont Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(FieldFont.Unknown);
		}

		return reader.ValueTextEquals("Courier"u8) ? FieldFont.Courier
			: reader.ValueTextEquals("Helvetica"u8) ? FieldFont.Helvetica
			: reader.ValueTextEquals("Times"u8) ? FieldFont.Times
			: reader.Unmatched(FieldFont.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		FieldFont value,
		JsonSerializerOptions options) {
		var fieldFont = value switch {
			FieldFont.Courier => "Courier",
			FieldFont.Helvetica => "Helvetica",
			FieldFont.Times => "Times",
			_ => null
		};

		writer.WriteStringValue(fieldFont);
	}
}