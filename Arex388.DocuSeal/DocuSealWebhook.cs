using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Arex388.DocuSeal;

/// <summary>
/// Parses the payloads DocuSeal posts to a webhook endpoint. It needs no client, account, or authorization token.
/// </summary>
public static class DocuSealWebhook {
	private static readonly UTF8Encoding _utf8 = new(false, true);

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

		var start = json.Length > 0
					&& json[0] == '\uFEFF'
			? 1
			: 0;
		byte[]? rented = null;

		//	Transcoded once into a pooled buffer, with an encoder that throws on invalid UTF-16 (a lone surrogate), so the
		//	string is read exactly as the UTF-8 overload reads its bytes.
		try {
			rented = ArrayPool<byte>.Shared.Rent(_utf8.GetMaxByteCount(json.Length - start));

			var count = _utf8.GetBytes(json, start, json.Length - start, rented, 0);

			return ParseUtf8(rented.AsSpan(0, count));
		} catch (Exception ex) when (IsMalformed(ex)) {
			return null;
		} finally {
			//	Cleared so the payload's bytes do not linger in the shared pool.
			if (rented is not null) {
				ArrayPool<byte>.Shared.Return(rented, clearArray: true);
			}
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
			return ParseUtf8(utf8Json);
		} catch (Exception ex) when (IsMalformed(ex)) {
			return null;
		}
	}

	//	============================================================================
	//	Utilities
	//	============================================================================

	//	JsonException covers malformed JSON (invalid UTF-8 included) and data that does not bind; ArgumentException covers a
	//	string with invalid UTF-16, such as a lone surrogate, that cannot be transcoded to UTF-8; InvalidOperationException
	//	covers an escaped lone surrogate (such as \uD800) in a property name or string value, which the reader throws for
	//	when it unescapes the token to compare it or to read it as a date.
	private static bool IsMalformed(
		Exception exception) => exception is JsonException or ArgumentException or InvalidOperationException;

	/// <summary>
	/// Reads the envelope in one forward pass with a <see cref="Utf8JsonReader"/>, which also validates the whole payload,
	/// and then deserializes the payload once into the subtype its <c>event_type</c> names. No <see cref="JsonDocument"/>
	/// is built. A member that appears more than once is read from its last occurrence, as
	/// <see cref="JsonElement.TryGetProperty(string, out JsonElement)"/> and the serializer read it.
	/// </summary>
	private static WebhookEvent? ParseUtf8(
		ReadOnlySpan<byte> utf8Json) {
		var reader = new Utf8JsonReader(utf8Json);

		if (!reader.Read()
			|| reader.TokenType != JsonTokenType.StartObject) {
			return null;
		}

		//	The scan keeps only where each envelope member's last occurrence starts; those values are decoded after it, so an
		//	earlier occurrence that cannot be decoded does not hide the last one.
		var dataStart = -1L;
		var dataToken = JsonTokenType.None;
		var eventTypeStart = -1L;
		var timestampStart = -1L;

		while (reader.Read()
			   && reader.TokenType == JsonTokenType.PropertyName) {
			if (reader.ValueTextEquals("event_type"u8)) {
				reader.Read();

				eventTypeStart = reader.TokenType == JsonTokenType.String
					? reader.TokenStartIndex
					: -1L;
			} else if (reader.ValueTextEquals("timestamp"u8)) {
				reader.Read();

				timestampStart = reader.TokenType == JsonTokenType.String
					? reader.TokenStartIndex
					: -1L;
			} else if (reader.ValueTextEquals("data"u8)) {
				reader.Read();

				dataStart = reader.TokenStartIndex;
				dataToken = reader.TokenType;
			} else {
				reader.Read();
			}

			reader.Skip();
		}

		//	The loop ends on the root's closing brace, so anything after it but whitespace is malformed.
		if (reader.Read()
			|| eventTypeStart < 0
			|| timestampStart < 0
			|| dataStart < 0) {
			return null;
		}

		var timestampReader = ReaderAt(utf8Json, timestampStart);

		if (!timestampReader.TryGetDateTime(out var timestampUtc)) {
			return null;
		}

		var options = DocuSealClient.SerializerOptions;
		var eventTypeReader = ReaderAt(utf8Json, eventTypeStart);
		var type = JsonSerializer.Deserialize<WebhookEventType>(ref eventTypeReader, options);

		if (type == WebhookEventType.Unknown) {
			var dataReader = ReaderAt(utf8Json, dataStart);

			return new UnknownWebhookEvent {
				Data = JsonElement.ParseValue(ref dataReader),
				RawType = eventTypeReader.GetString()!,
				TimestampUtc = timestampUtc,
				Type = WebhookEventType.Unknown
			};
		}

		if (dataToken != JsonTokenType.StartObject) {
			return null;
		}

		return type switch {
			WebhookEventType.FormCompleted
				or WebhookEventType.FormDeclined
				or WebhookEventType.FormStarted
				or WebhookEventType.FormViewed => JsonSerializer.Deserialize<FormWebhookEvent>(utf8Json, options),
			WebhookEventType.SubmissionArchived => JsonSerializer.Deserialize<SubmissionArchivedWebhookEvent>(utf8Json, options),
			WebhookEventType.SubmissionCompleted
				or WebhookEventType.SubmissionCreated
				or WebhookEventType.SubmissionExpired => JsonSerializer.Deserialize<SubmissionWebhookEvent>(utf8Json, options),
			WebhookEventType.TemplateArchived => JsonSerializer.Deserialize<TemplateArchivedWebhookEvent>(utf8Json, options),
			WebhookEventType.TemplateCreated
				or WebhookEventType.TemplateUpdated => JsonSerializer.Deserialize<TemplateWebhookEvent>(utf8Json, options),
			_ => null
		};
	}

	//	A reader positioned on the value that starts at the given offset of the payload, which the forward pass has already validated.
	private static Utf8JsonReader ReaderAt(
		ReadOnlySpan<byte> utf8Json,
		long start) {
		var reader = new Utf8JsonReader(utf8Json.Slice((int)start));

		reader.Read();

		return reader;
	}
}