using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class WebhookEventTypeJsonConverter :
	JsonConverter<WebhookEventType> {
	public override WebhookEventType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"form.completed" => WebhookEventType.FormCompleted,
			"form.declined" => WebhookEventType.FormDeclined,
			"form.started" => WebhookEventType.FormStarted,
			"form.viewed" => WebhookEventType.FormViewed,
			"submission.archived" => WebhookEventType.SubmissionArchived,
			"submission.completed" => WebhookEventType.SubmissionCompleted,
			"submission.created" => WebhookEventType.SubmissionCreated,
			"submission.expired" => WebhookEventType.SubmissionExpired,
			"template.archived" => WebhookEventType.TemplateArchived,
			"template.created" => WebhookEventType.TemplateCreated,
			"template.updated" => WebhookEventType.TemplateUpdated,
			_ => WebhookEventType.Unknown
		};

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