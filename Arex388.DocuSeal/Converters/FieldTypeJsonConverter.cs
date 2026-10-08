using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class FieldTypeJsonConverter :
	JsonConverter<FieldType> {
	public override FieldType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(FieldType.Unknown);
		}

		return reader.ValueTextEquals("cells"u8) ? FieldType.Cells
			: reader.ValueTextEquals("checkbox"u8) ? FieldType.Checkbox
			: reader.ValueTextEquals("date"u8) ? FieldType.Date
			: reader.ValueTextEquals("file"u8) ? FieldType.File
			: reader.ValueTextEquals("heading"u8) ? FieldType.Heading
			: reader.ValueTextEquals("image"u8) ? FieldType.Image
			: reader.ValueTextEquals("initials"u8) ? FieldType.Initials
			: reader.ValueTextEquals("kba"u8) ? FieldType.Kba
			: reader.ValueTextEquals("multiple"u8) ? FieldType.Multiple
			: reader.ValueTextEquals("number"u8) ? FieldType.Number
			: reader.ValueTextEquals("payment"u8) ? FieldType.Payment
			: reader.ValueTextEquals("phone"u8) ? FieldType.Phone
			: reader.ValueTextEquals("radio"u8) ? FieldType.Radio
			: reader.ValueTextEquals("select"u8) ? FieldType.Select
			: reader.ValueTextEquals("signature"u8) ? FieldType.Signature
			: reader.ValueTextEquals("stamp"u8) ? FieldType.Stamp
			: reader.ValueTextEquals("strikethrough"u8) ? FieldType.Strikethrough
			: reader.ValueTextEquals("text"u8) ? FieldType.Text
			: reader.ValueTextEquals("verification"u8) ? FieldType.Verification
			: reader.Unmatched(FieldType.Unknown);
	}

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