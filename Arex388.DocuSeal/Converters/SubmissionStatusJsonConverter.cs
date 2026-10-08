using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class SubmissionStatusJsonConverter :
	JsonConverter<SubmissionStatus> {
	public override SubmissionStatus Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"completed" => SubmissionStatus.Completed,
			"declined" => SubmissionStatus.Declined,
			"expired" => SubmissionStatus.Expired,
			"pending" => SubmissionStatus.Pending,
			_ => SubmissionStatus.Unknown
		};

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