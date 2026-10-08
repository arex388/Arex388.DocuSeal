using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldAlignJsonConverter :
	JsonConverter<FieldAlign> {
	public override FieldAlign Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"center" => FieldAlign.Center,
			"left" => FieldAlign.Left,
			"right" => FieldAlign.Right,
			_ => FieldAlign.Unknown
		};

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