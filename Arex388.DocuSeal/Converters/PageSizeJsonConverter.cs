using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class PageSizeJsonConverter :
	JsonConverter<PageSize> {
	public override PageSize Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"A0" => PageSize.A0,
			"A1" => PageSize.A1,
			"A2" => PageSize.A2,
			"A3" => PageSize.A3,
			"A4" => PageSize.A4,
			"A5" => PageSize.A5,
			"A6" => PageSize.A6,
			"Ledger" => PageSize.Ledger,
			"Legal" => PageSize.Legal,
			"Letter" => PageSize.Letter,
			"Tabloid" => PageSize.Tabloid,
			_ => PageSize.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		PageSize value,
		JsonSerializerOptions options) {
		var pageSize = value switch {
			PageSize.A0 => "A0",
			PageSize.A1 => "A1",
			PageSize.A2 => "A2",
			PageSize.A3 => "A3",
			PageSize.A4 => "A4",
			PageSize.A5 => "A5",
			PageSize.A6 => "A6",
			PageSize.Ledger => "Ledger",
			PageSize.Legal => "Legal",
			PageSize.Letter => "Letter",
			PageSize.Tabloid => "Tabloid",
			_ => null
		};

		writer.WriteStringValue(pageSize);
	}
}