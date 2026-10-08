using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmissionStatusJsonConverter :
	JsonConverter<SubmissionStatus> {
	public override SubmissionStatus Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(SubmissionStatus.Unknown);
		}

		return reader.ValueTextEquals("completed"u8) ? SubmissionStatus.Completed
			: reader.ValueTextEquals("declined"u8) ? SubmissionStatus.Declined
			: reader.ValueTextEquals("expired"u8) ? SubmissionStatus.Expired
			: reader.ValueTextEquals("pending"u8) ? SubmissionStatus.Pending
			: reader.Unmatched(SubmissionStatus.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		SubmissionStatus value,
		JsonSerializerOptions options) => writer.WriteStringValue(GetToken(value));

	//	Shared with ListSubmissions' status query parameter, so the body and the query use one token table.
	internal static string? GetToken(
		SubmissionStatus value) => value switch {
			SubmissionStatus.Completed => "completed",
			SubmissionStatus.Declined => "declined",
			SubmissionStatus.Expired => "expired",
			SubmissionStatus.Pending => "pending",
			_ => null
		};
}