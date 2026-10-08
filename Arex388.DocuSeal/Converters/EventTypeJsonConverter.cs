using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class EventTypeJsonConverter :
	JsonConverter<EventType> {
	public override EventType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) => reader.GetString() switch {
			"api_complete_form" => EventType.ApiCompletedForm,
			"bounce_email" => EventType.BouncedEmail,
			"click_email" => EventType.ClickedEmail,
			"click_sms" => EventType.ClickedSms,
			"complete_form" => EventType.CompletedForm,
			"complete_verification" => EventType.CompletedVerification,
			"complaint_email" => EventType.ComplainedEmail,
			"decline_form" => EventType.DeclinedForm,
			"invite_party" => EventType.InvitedParty,
			"open_email" => EventType.OpenedEmail,
			"send_email" => EventType.SentEmail,
			"send_reminder_email" => EventType.SentReminderEmail,
			"send_sms" => EventType.SentSms,
			"send_2fa_sms" => EventType.SentTwoFactorSms,
			"start_form" => EventType.StartedForm,
			"start_verification" => EventType.StartedVerification,
			"phone_verified" => EventType.VerifiedPhone,
			"view_form" => EventType.ViewedForm,
			_ => EventType.Unknown
		};

	public override void Write(
		Utf8JsonWriter writer,
		EventType value,
		JsonSerializerOptions options) {
		var eventType = value switch {
			EventType.ApiCompletedForm => "api_complete_form",
			EventType.BouncedEmail => "bounce_email",
			EventType.ClickedEmail => "click_email",
			EventType.ClickedSms => "click_sms",
			EventType.CompletedForm => "complete_form",
			EventType.CompletedVerification => "complete_verification",
			EventType.ComplainedEmail => "complaint_email",
			EventType.DeclinedForm => "decline_form",
			EventType.InvitedParty => "invite_party",
			EventType.OpenedEmail => "open_email",
			EventType.SentEmail => "send_email",
			EventType.SentReminderEmail => "send_reminder_email",
			EventType.SentSms => "send_sms",
			EventType.SentTwoFactorSms => "send_2fa_sms",
			EventType.StartedForm => "start_form",
			EventType.StartedVerification => "start_verification",
			EventType.VerifiedPhone => "phone_verified",
			EventType.ViewedForm => "view_form",
			_ => null
		};

		writer.WriteStringValue(eventType);
	}
}