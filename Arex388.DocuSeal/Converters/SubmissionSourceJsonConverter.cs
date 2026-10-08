using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmissionSourceJsonConverter :
	JsonConverter<SubmissionSource> {
	public override SubmissionSource Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(SubmissionSource.Unknown);
		}

		return reader.ValueTextEquals("api"u8) ? SubmissionSource.Api
			: reader.ValueTextEquals("bulk"u8) ? SubmissionSource.Bulk
			: reader.ValueTextEquals("embed"u8) ? SubmissionSource.Embed
			: reader.ValueTextEquals("invite"u8) ? SubmissionSource.Invite
			: reader.ValueTextEquals("link"u8) ? SubmissionSource.Link
			: reader.ValueTextEquals("mcp"u8) ? SubmissionSource.Mcp
			: reader.ValueTextEquals("self"u8) ? SubmissionSource.Self
			: reader.Unmatched(SubmissionSource.Unknown);
	}

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