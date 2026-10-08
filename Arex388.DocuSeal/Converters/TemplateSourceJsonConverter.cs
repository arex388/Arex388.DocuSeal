using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class TemplateSourceJsonConverter :
	JsonConverter<TemplateSource> {
	public override TemplateSource Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"api" => TemplateSource.Api,
			"embed" => TemplateSource.Embed,
			"mcp" => TemplateSource.Mcp,
			"native" => TemplateSource.Native,
			_ => TemplateSource.Unknown
		};

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