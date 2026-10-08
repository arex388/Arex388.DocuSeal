using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldFontJsonConverter :
	JsonConverter<FieldFont> {
	public override FieldFont Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"Courier" => FieldFont.Courier,
			"Helvetica" => FieldFont.Helvetica,
			"Times" => FieldFont.Times,
			_ => FieldFont.Unknown
		};

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