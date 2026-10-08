using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class PageSizeJsonConverter :
	JsonConverter<PageSize> {
	public override PageSize Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(PageSize.Unknown);
		}

		return reader.ValueTextEquals("A0"u8) ? PageSize.A0
			: reader.ValueTextEquals("A1"u8) ? PageSize.A1
			: reader.ValueTextEquals("A2"u8) ? PageSize.A2
			: reader.ValueTextEquals("A3"u8) ? PageSize.A3
			: reader.ValueTextEquals("A4"u8) ? PageSize.A4
			: reader.ValueTextEquals("A5"u8) ? PageSize.A5
			: reader.ValueTextEquals("A6"u8) ? PageSize.A6
			: reader.ValueTextEquals("Ledger"u8) ? PageSize.Ledger
			: reader.ValueTextEquals("Legal"u8) ? PageSize.Legal
			: reader.ValueTextEquals("Letter"u8) ? PageSize.Letter
			: reader.ValueTextEquals("Tabloid"u8) ? PageSize.Tabloid
			: reader.Unmatched(PageSize.Unknown);
	}

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