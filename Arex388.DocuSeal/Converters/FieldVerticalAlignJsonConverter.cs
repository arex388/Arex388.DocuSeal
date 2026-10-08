using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldVerticalAlignJsonConverter :
	JsonConverter<FieldVerticalAlign> {
	public override FieldVerticalAlign Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"bottom" => FieldVerticalAlign.Bottom,
			"center" => FieldVerticalAlign.Center,
			"top" => FieldVerticalAlign.Top,
			_ => FieldVerticalAlign.Unknown
		};

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