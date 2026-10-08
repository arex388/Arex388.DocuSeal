using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldVerticalAlignJsonConverter :
	JsonConverter<FieldVerticalAlign> {
	public override FieldVerticalAlign Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(FieldVerticalAlign.Unknown);
		}

		return reader.ValueTextEquals("bottom"u8) ? FieldVerticalAlign.Bottom
			: reader.ValueTextEquals("center"u8) ? FieldVerticalAlign.Center
			: reader.ValueTextEquals("top"u8) ? FieldVerticalAlign.Top
			: reader.Unmatched(FieldVerticalAlign.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		FieldVerticalAlign value,
		JsonSerializerOptions options) {
		var fieldVerticalAlign = value switch {
			FieldVerticalAlign.Bottom => "bottom",
			FieldVerticalAlign.Center => "center",
			FieldVerticalAlign.Top => "top",
			_ => null
		};

		writer.WriteStringValue(fieldVerticalAlign);
	}
}