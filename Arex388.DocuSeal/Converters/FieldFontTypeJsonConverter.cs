using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldFontTypeJsonConverter :
	JsonConverter<FieldFontType> {
	public override FieldFontType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"bold" => FieldFontType.Bold,
			"bold_italic" => FieldFontType.BoldItalic,
			"italic" => FieldFontType.Italic,
			_ => FieldFontType.Unknown
		};

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