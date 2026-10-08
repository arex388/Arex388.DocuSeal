using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class TemplateSourceJsonConverter :
	JsonConverter<TemplateSource> {
	public override TemplateSource Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(TemplateSource.Unknown);
		}

		return reader.ValueTextEquals("api"u8) ? TemplateSource.Api
			: reader.ValueTextEquals("embed"u8) ? TemplateSource.Embed
			: reader.ValueTextEquals("mcp"u8) ? TemplateSource.Mcp
			: reader.ValueTextEquals("native"u8) ? TemplateSource.Native
			: reader.Unmatched(TemplateSource.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		TemplateSource value,
		JsonSerializerOptions options) {
		var templateSource = value switch {
			TemplateSource.Api => "api",
			TemplateSource.Embed => "embed",
			TemplateSource.Mcp => "mcp",
			TemplateSource.Native => "native",
			_ => null
		};

		writer.WriteStringValue(templateSource);
	}
}