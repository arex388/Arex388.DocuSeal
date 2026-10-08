using System.Text.Json;

namespace Arex388.DocuSeal;

/// <summary>
/// Parses the payloads DocuSeal posts to a webhook endpoint. It needs no client, account, or authorization token.
/// </summary>
public static class DocuSealWebhook {
	/// <summary>
	/// Parse a webhook payload.
	/// </summary>
	/// <param name="json">The webhook request's body.</param>
	/// <returns>The webhook event as the subtype matching its <c>event_type</c>: <see cref="FormWebhookEvent"/>, <see cref="SubmissionWebhookEvent"/>, <see cref="SubmissionArchivedWebhookEvent"/>, <see cref="TemplateWebhookEvent"/>, or <see cref="TemplateArchivedWebhookEvent"/>; an <see cref="UnknownWebhookEvent"/> for an <c>event_type</c> the client does not know; or <see langword="null"/>, without throwing, when the payload cannot be read: the body is not valid JSON (invalid UTF-16 or UTF-8 included), its root is not an object, <c>event_type</c> is missing or not a string, <c>timestamp</c> is missing or not an ISO 8601 date string, <c>data</c> is missing, or a known <c>event_type</c>'s <c>data</c> is not an object or does not bind to its model. A <see langword="null"/> result means the payload could not be parsed, not that DocuSeal did not send it.</returns>
	public static WebhookEvent? Parse(
		string json) {
		if (json is null) {
			return null;
		}

		if (json.Length > 0
			&& json[0] == '\uFEFF') {
			json = json.Substring(1);
		}

		try {
			using var document = JsonDocument.Parse(json);

			return ParseRoot(document.RootElement);
		} catch (Exception ex) when (IsMalformed(ex)) {
			return null;
		}
	}

	/// <summary>
	/// Parse a webhook payload.
	/// </summary>
	/// <param name="utf8Json">The webhook request's body, as UTF-8 bytes.</param>
	/// <returns>The webhook event as the subtype matching its <c>event_type</c>: <see cref="FormWebhookEvent"/>, <see cref="SubmissionWebhookEvent"/>, <see cref="SubmissionArchivedWebhookEvent"/>, <see cref="TemplateWebhookEvent"/>, or <see cref="TemplateArchivedWebhookEvent"/>; an <see cref="UnknownWebhookEvent"/> for an <c>event_type</c> the client does not know; or <see langword="null"/>, without throwing, when the payload cannot be read: the body is not valid JSON (invalid UTF-16 or UTF-8 included), its root is not an object, <c>event_type</c> is missing or not a string, <c>timestamp</c> is missing or not an ISO 8601 date string, <c>data</c> is missing, or a known <c>event_type</c>'s <c>data</c> is not an object or does not bind to its model. A <see langword="null"/> result means the payload could not be parsed, not that DocuSeal did not send it.</returns>
	public static WebhookEvent? Parse(
		ReadOnlySpan<byte> utf8Json) {
		if (utf8Json.StartsWith("\uFEFF"u8)) {
			utf8Json = utf8Json.Slice(3);
		}

		try {
			var reader = new Utf8JsonReader(utf8Json);

			using var document = JsonDocument.ParseValue(ref reader);

			//	ParseValue stops after the first value, so anything after it but whitespace is malformed.
			if (reader.Read()) {
				return null;
			}

			return ParseRoot(document.RootElement);
		} catch (Exception ex) when (IsMalformed(ex)) {
			return null;
		}
	}

	//	============================================================================
	//	Utilities
	//	============================================================================

	//	JsonException covers malformed JSON (invalid UTF-8 included) and data that does not bind; ArgumentException covers a
	//	string with invalid UTF-16, such as a lone surrogate, that JsonDocument cannot transcode to UTF-8.
	private static bool IsMalformed(
		Exception exception) => exception is JsonException or ArgumentException;

	private static WebhookEvent? ParseRoot(
		JsonElement root) {
		if (root.ValueKind != JsonValueKind.Object
			|| !root.TryGetProperty("event_type", out var eventType)
			|| eventType.ValueKind != JsonValueKind.String
			|| !root.TryGetProperty("timestamp", out var timestamp)
			|| timestamp.ValueKind != JsonValueKind.String
			|| !timestamp.TryGetDateTime(out var timestampUtc)
			|| !root.TryGetProperty("data", out var data)) {
			return null;
		}

		var options = DocuSealClient.SerializerOptions;
		var type = eventType.Deserialize<WebhookEventType>(options);

		if (type == WebhookEventType.Unknown) {
			return new UnknownWebhookEvent {
				Data = data.Clone(),
				RawType = eventType.GetString()!,
				TimestampUtc = timestampUtc,
				Type = WebhookEventType.Unknown
			};
		}

		if (data.ValueKind != JsonValueKind.Object) {
			return null;
		}

		return type switch {
			WebhookEventType.FormCompleted
				or WebhookEventType.FormDeclined
				or WebhookEventType.FormStarted
				or WebhookEventType.FormViewed => root.Deserialize<FormWebhookEvent>(options),
			WebhookEventType.SubmissionArchived => root.Deserialize<SubmissionArchivedWebhookEvent>(options),
			WebhookEventType.SubmissionCompleted
				or WebhookEventType.SubmissionCreated
				or WebhookEventType.SubmissionExpired => root.Deserialize<SubmissionWebhookEvent>(options),
			WebhookEventType.TemplateArchived => root.Deserialize<TemplateArchivedWebhookEvent>(options),
			WebhookEventType.TemplateCreated
				or WebhookEventType.TemplateUpdated => root.Deserialize<TemplateWebhookEvent>(options),
			_ => null
		};
	}
}