using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldFontTypeJsonConverter :
	JsonConverter<FieldFontType> {
	public override FieldFontType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(FieldFontType.Unknown);
		}

		return reader.ValueTextEquals("bold"u8) ? FieldFontType.Bold
			: reader.ValueTextEquals("bold_italic"u8) ? FieldFontType.BoldItalic
			: reader.ValueTextEquals("italic"u8) ? FieldFontType.Italic
			: reader.Unmatched(FieldFontType.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		FieldFontType value,
		JsonSerializerOptions options) {
		var fieldFontType = value switch {
			FieldFontType.Bold => "bold",
			FieldFontType.BoldItalic => "bold_italic",
			FieldFontType.Italic => "italic",
			_ => null
		};

		writer.WriteStringValue(fieldFontType);
	}
}