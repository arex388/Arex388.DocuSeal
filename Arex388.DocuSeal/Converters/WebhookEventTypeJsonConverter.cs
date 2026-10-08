using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class WebhookEventTypeJsonConverter :
	JsonConverter<WebhookEventType> {
	public override WebhookEventType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(WebhookEventType.Unknown);
		}

		return reader.ValueTextEquals("form.completed"u8) ? WebhookEventType.FormCompleted
			: reader.ValueTextEquals("form.declined"u8) ? WebhookEventType.FormDeclined
			: reader.ValueTextEquals("form.started"u8) ? WebhookEventType.FormStarted
			: reader.ValueTextEquals("form.viewed"u8) ? WebhookEventType.FormViewed
			: reader.ValueTextEquals("submission.archived"u8) ? WebhookEventType.SubmissionArchived
			: reader.ValueTextEquals("submission.completed"u8) ? WebhookEventType.SubmissionCompleted
			: reader.ValueTextEquals("submission.created"u8) ? WebhookEventType.SubmissionCreated
			: reader.ValueTextEquals("submission.expired"u8) ? WebhookEventType.SubmissionExpired
			: reader.ValueTextEquals("template.archived"u8) ? WebhookEventType.TemplateArchived
			: reader.ValueTextEquals("template.created"u8) ? WebhookEventType.TemplateCreated
			: reader.ValueTextEquals("template.updated"u8) ? WebhookEventType.TemplateUpdated
			: reader.Unmatched(WebhookEventType.Unknown);
	}

	public override void Write(
		Utf8JsonWriter writer,
		WebhookEventType value,
		JsonSerializerOptions options) {
		var webhookEventType = value switch {
			WebhookEventType.FormCompleted => "form.completed",
			WebhookEventType.FormDeclined => "form.declined",
			WebhookEventType.FormStarted => "form.started",
			WebhookEventType.FormViewed => "form.viewed",
			WebhookEventType.SubmissionArchived => "submission.archived",
			WebhookEventType.SubmissionCompleted => "submission.completed",
			WebhookEventType.SubmissionCreated => "submission.created",
			WebhookEventType.SubmissionExpired => "submission.expired",
			WebhookEventType.TemplateArchived => "template.archived",
			WebhookEventType.TemplateCreated => "template.created",
			WebhookEventType.TemplateUpdated => "template.updated",
			_ => null
		};

		writer.WriteStringValue(webhookEventType);
	}
}