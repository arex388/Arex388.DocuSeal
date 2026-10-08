using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class CurrencyJsonConverter :
	JsonConverter<Currency> {
	public override Currency Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"AUD" => Currency.Aud,
			"CAD" => Currency.Cad,
			"CHF" => Currency.Chf,
			"EUR" => Currency.Eur,
			"GBP" => Currency.Gbp,
			"SEK" => Currency.Sek,
			"USD" => Currency.Usd,
			_ => Currency.Unknown
		};

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