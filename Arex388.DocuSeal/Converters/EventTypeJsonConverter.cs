using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

internal sealed class EventTypeJsonConverter :
	JsonConverter<EventType> {
	public override EventType Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (reader.TokenType != JsonTokenType.String) {
			return reader.Unmatched(EventType.Unknown);
		}

		return reader.ValueTextEquals("api_complete_form"u8) ? EventType.ApiCompletedForm
			: reader.ValueTextEquals("bounce_email"u8) ? EventType.BouncedEmail
			: reader.ValueTextEquals("click_email"u8) ? EventType.ClickedEmail
			: reader.ValueTextEquals("click_sms"u8) ? EventType.ClickedSms
			: reader.ValueTextEquals("complete_form"u8) ? EventType.CompletedForm
			: reader.ValueTextEquals("complete_verification"u8) ? EventType.CompletedVerification
			: reader.ValueTextEquals("complaint_email"u8) ? EventType.ComplainedEmail
			: reader.ValueTextEquals("decline_form"u8) ? EventType.DeclinedForm
			: reader.ValueTextEquals("invite_party"u8) ? EventType.InvitedParty
			: reader.ValueTextEquals("open_email"u8) ? EventType.OpenedEmail
			: reader.ValueTextEquals("send_email"u8) ? EventType.SentEmail
			: reader.ValueTextEquals("send_reminder_email"u8) ? EventType.SentReminderEmail
			: reader.ValueTextEquals("send_sms"u8) ? EventType.SentSms
			: reader.ValueTextEquals("send_2fa_sms"u8) ? EventType.SentTwoFactorSms
			: reader.ValueTextEquals("start_form"u8) ? EventType.StartedForm
			: reader.ValueTextEquals("start_verification"u8) ? EventType.StartedVerification
			: reader.ValueTextEquals("phone_verified"u8) ? EventType.VerifiedPhone
			: reader.ValueTextEquals("view_form"u8) ? EventType.ViewedForm
			: reader.Unmatched(EventType.Unknown);
	}

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