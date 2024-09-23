using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmitterOrderJsonConverter :
	JsonConverter<SubmitterOrder> {
	public override SubmitterOrder Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"preserved" => SubmitterOrder.Preserved,
			"random" => SubmitterOrder.Random,
			_ => SubmitterOrder.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		SubmitterOrder value,
		JsonSerializerOptions options) {
		var order = value switch {
			SubmitterOrder.Preserved => "preserved",
			SubmitterOrder.Random => "random",
			_ => null
		};

		writer.WriteStringValue(order);
	}
}