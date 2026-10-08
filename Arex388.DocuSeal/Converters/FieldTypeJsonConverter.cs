using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldTypeJsonConverter :
	JsonConverter<FieldType> {
	public override FieldType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"cells" => FieldType.Cells,
			"checkbox" => FieldType.Checkbox,
			"date" => FieldType.Date,
			"file" => FieldType.File,
			"heading" => FieldType.Heading,
			"image" => FieldType.Image,
			"initials" => FieldType.Initials,
			"kba" => FieldType.Kba,
			"multiple" => FieldType.Multiple,
			"number" => FieldType.Number,
			"payment" => FieldType.Payment,
			"phone" => FieldType.Phone,
			"radio" => FieldType.Radio,
			"select" => FieldType.Select,
			"signature" => FieldType.Signature,
			"stamp" => FieldType.Stamp,
			"strikethrough" => FieldType.Strikethrough,
			"text" => FieldType.Text,
			"verification" => FieldType.Verification,
			_ => FieldType.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		FieldType value,
		JsonSerializerOptions options) {
		var fieldType = value switch {
			FieldType.Cells => "cells",
			FieldType.Checkbox => "checkbox",
			FieldType.Date => "date",
			FieldType.File => "file",
			FieldType.Heading => "heading",
			FieldType.Image => "image",
			FieldType.Initials => "initials",
			FieldType.Kba => "kba",
			FieldType.Multiple => "multiple",
			FieldType.Number => "number",
			FieldType.Payment => "payment",
			FieldType.Phone => "phone",
			FieldType.Radio => "radio",
			FieldType.Select => "select",
			FieldType.Signature => "signature",
			FieldType.Stamp => "stamp",
			FieldType.Strikethrough => "strikethrough",
			FieldType.Text => "text",
			FieldType.Verification => "verification",
			_ => null
		};

		writer.WriteStringValue(fieldType);
	}
}