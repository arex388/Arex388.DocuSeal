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
			"image" => FieldType.Image,
			"initials" => FieldType.Initials,
			"multiple" => FieldType.Multiple,
			"payment" => FieldType.Payment,
			"phone" => FieldType.Phone,
			"radio" => FieldType.Radio,
			"select" => FieldType.Select,
			"signature" => FieldType.Signature,
			"stamp" => FieldType.Stamp,
			_ => FieldType.Text
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
			FieldType.Image => "image",
			FieldType.Initials => "initials",
			FieldType.Multiple => "multiple",
			FieldType.Payment => "payment",
			FieldType.Phone => "phone",
			FieldType.Radio => "radio",
			FieldType.Select => "select",
			FieldType.Signature => "signature",
			FieldType.Stamp => "stamp",
			_ => "text"
		};

		writer.WriteStringValue(fieldType);
	}
}