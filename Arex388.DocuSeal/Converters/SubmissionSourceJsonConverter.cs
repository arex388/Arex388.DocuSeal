using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmissionSourceJsonConverter :
	JsonConverter<SubmissionSource> {
	public override SubmissionSource Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"api" => SubmissionSource.Api,
			"bulk" => SubmissionSource.Bulk,
			"embed" => SubmissionSource.Embed,
			"invite" => SubmissionSource.Invite,
			"link" => SubmissionSource.Link,
			"mcp" => SubmissionSource.Mcp,
			"self" => SubmissionSource.Self,
			_ => SubmissionSource.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		SubmissionSource value,
		JsonSerializerOptions options) {
		var submissionSource = value switch {
			SubmissionSource.Api => "api",
			SubmissionSource.Bulk => "bulk",
			SubmissionSource.Embed => "embed",
			SubmissionSource.Invite => "invite",
			SubmissionSource.Link => "link",
			SubmissionSource.Mcp => "mcp",
			SubmissionSource.Self => "self",
			_ => null
		};

		writer.WriteStringValue(submissionSource);
	}
}