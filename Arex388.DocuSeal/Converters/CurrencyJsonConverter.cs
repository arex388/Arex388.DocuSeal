using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class CurrencyJsonConverter :
	JsonConverter<Currency> {
	public override Currency Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(Currency.Unknown);
		}

		return reader.ValueTextEquals("AUD"u8) ? Currency.Aud
			: reader.ValueTextEquals("CAD"u8) ? Currency.Cad
			: reader.ValueTextEquals("CHF"u8) ? Currency.Chf
			: reader.ValueTextEquals("EUR"u8) ? Currency.Eur
			: reader.ValueTextEquals("GBP"u8) ? Currency.Gbp
			: reader.ValueTextEquals("SEK"u8) ? Currency.Sek
			: reader.ValueTextEquals("USD"u8) ? Currency.Usd
			: reader.Unmatched(Currency.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		Currency value,
		JsonSerializerOptions options) {
		var currency = value switch {
			Currency.Aud => "AUD",
			Currency.Cad => "CAD",
			Currency.Chf => "CHF",
			Currency.Eur => "EUR",
			Currency.Gbp => "GBP",
			Currency.Sek => "SEK",
			Currency.Usd => "USD",
			_ => null
		};

		writer.WriteStringValue(currency);
	}
}