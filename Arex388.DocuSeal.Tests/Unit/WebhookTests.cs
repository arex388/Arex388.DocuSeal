using FluentAssertions;
using System.Text;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Parses the OpenAPI spec's eleven webhook example payloads
/// (<c>TestData/Spec/webhook-*.json</c>) through both
/// <see cref="DocuSealWebhook"/> overloads, and asserts that malformed and
/// unknown payloads return without throwing.
/// </summary>
public sealed class WebhookTests {
	private static readonly string _specDirectory = Path.Combine(AppContext.BaseDirectory, "Spec");

	private static string Spec(
		string name) => File.ReadAllText(Path.Combine(_specDirectory, $"webhook-{name}.json"));

	private static DateTime Utc(
		int year,
		int month,
		int day,
		int hour,
		int minute,
		int second,
		int millisecond) => new(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);

	/// <summary>
	/// Parses the payload through both overloads, asserts they agree on the
	/// subtype and the envelope, and returns the string overload's result.
	/// </summary>
	private static WebhookEvent? ParseBoth(
		string json) {
		var fromString = DocuSealWebhook.Parse(json);
		var fromBytes = DocuSealWebhook.Parse(Encoding.UTF8.GetBytes(json));

		if (fromString is null) {
			fromBytes.Should().BeNull();
		} else {
			fromBytes.Should().NotBeNull();
			fromBytes!.GetType().Should().Be(fromString.GetType());
			fromBytes.Type.Should().Be(fromString.Type);
			fromBytes.TimestampUtc.Should().Be(fromString.TimestampUtc);
		}

		return fromString;
	}

	//	============================================================================
	//	Spec Examples
	//	============================================================================

	[Theory]
	[InlineData("form-viewed", WebhookEventType.FormViewed, typeof(FormWebhookEvent), 1)]
	[InlineData("form-started", WebhookEventType.FormStarted, typeof(FormWebhookEvent), 1)]
	[InlineData("form-completed", WebhookEventType.FormCompleted, typeof(FormWebhookEvent), 1)]
	[InlineData("form-declined", WebhookEventType.FormDeclined, typeof(FormWebhookEvent), 1)]
	[InlineData("submission-created", WebhookEventType.SubmissionCreated, typeof(SubmissionWebhookEvent), 1)]
	[InlineData("submission-completed", WebhookEventType.SubmissionCompleted, typeof(SubmissionWebhookEvent), 1)]
	[InlineData("submission-expired", WebhookEventType.SubmissionExpired, typeof(SubmissionWebhookEvent), 1)]
	[InlineData("submission-archived", WebhookEventType.SubmissionArchived, typeof(SubmissionArchivedWebhookEvent), 1)]
	[InlineData("template-created", WebhookEventType.TemplateCreated, typeof(TemplateWebhookEvent), 1)]
	[InlineData("template-updated", WebhookEventType.TemplateUpdated, typeof(TemplateWebhookEvent), 1)]
	[InlineData("template-archived", WebhookEventType.TemplateArchived, typeof(TemplateArchivedWebhookEvent), 1)]
	public void Parse_SpecExample_ReturnsSubtypeWithData(
		string name,
		WebhookEventType type,
		Type subtype,
		int id) {
		var webhookEvent = ParseBoth(Spec(name));

		webhookEvent.Should().NotBeNull();
		webhookEvent!.GetType().Should().Be(subtype);
		webhookEvent.Type.Should().Be(type);
		webhookEvent.TimestampUtc.Should().NotBe(default);

		object? dataId = webhookEvent switch {
			FormWebhookEvent e => e.Data.Id,
			SubmissionWebhookEvent e => e.Data.Id,
			SubmissionArchivedWebhookEvent e => e.Data.Id,
			TemplateWebhookEvent e => e.Data.Id,
			TemplateArchivedWebhookEvent e => e.Data.Id,
			_ => null
		};
		object? expectedId = webhookEvent switch {
			FormWebhookEvent => new SubmitterId(id),
			SubmissionWebhookEvent or SubmissionArchivedWebhookEvent => new SubmissionId(id),
			TemplateWebhookEvent or TemplateArchivedWebhookEvent => new TemplateId(id),
			_ => null
		};

		dataId.Should().NotBeNull($"the {name} example carries data").And.Be(expectedId);
	}

	[Fact]
	public void Parse_FormViewed_BindsData() {
		var webhookEvent = (FormWebhookEvent)DocuSealWebhook.Parse(Spec("form-viewed"))!;

		webhookEvent.TimestampUtc.Should().Be(Utc(2023, 9, 24, 13, 48, 36, 0));

		var data = webhookEvent.Data;

		data.Id.Should().Be(new SubmitterId(1));
		data.Email.Should().Be("john.doe@example.com");
		data.UserAgent.Should().StartWith("Mozilla/5.0 (Macintosh;");
		data.IpAddress.Should().Be("132.216.88.83");
		data.SentAtUtc.Should().Be(Utc(2023, 8, 20, 10, 9, 5, 459));
		data.OpenedAtUtc.Should().Be(Utc(2023, 8, 20, 10, 10, 0, 451));
		data.CompletedAtUtc.Should().Be(Utc(2023, 8, 20, 10, 12, 47, 579));
		data.DeclinedAtUtc.Should().BeNull();
		data.CreatedAtUtc.Should().Be(Utc(2023, 8, 20, 10, 9, 2, 459));
		data.UpdatedAtUtc.Should().Be(Utc(2023, 8, 20, 10, 12, 47, 907));
		data.Name.Should().BeNull();
		data.Phone.Should().BeNull();
		data.Role.Should().Be("First Party");
		data.ExternalId.Should().BeNull();
		data.DeclineReason.Should().BeNull();
		data.Status.Should().Be(SubmitterStatus.Completed);
		data.Preferences!["send_email"]!.GetValue<bool>().Should().BeTrue();
		data.Preferences["send_sms"]!.GetValue<bool>().Should().BeFalse();
		data.Metadata!["customData"]!.GetValue<string>().Should().Be("custom value");
		data.AuditUrl.Should().Be(new Uri("https://docuseal.com/blobs/proxy/eyJfcmFpbHMiOnsib/audit-log.pdf"));
		data.SubmissionUrl.Should().Be(new Uri("https://docuseal.com/e/N5JsdkFGPeQF7J"));
		data.SubmissionId.Should().Be(default(SubmissionId), "the spec example omits submission_id");

		var submission = data.Submission!;

		submission.Id.Should().Be(new SubmissionId(12));
		submission.AuditUrl.Should().Be(new Uri("https://docuseal.com/blobs/proxy/eyJfcmFpbHMiOnsib/audit-log.pdf"));
		submission.CombinedDocumentUrl.Should().Be(new Uri("https://docuseal.com/blobs/proxy/eyJfcmFpbHMiOnsib/document.pdf"));
		submission.Status.Should().Be(SubmissionStatus.Completed);
		submission.Url.Should().Be(new Uri("https://docuseal.com/e/N5JsdkFGPeQF7J"));
		submission.Variables!["custom_variable"]!.GetValue<string>().Should().Be("value");
		submission.CreatedAtUtc.Should().Be(Utc(2023, 8, 20, 10, 9, 5, 258));

		var template = data.Template!;

		template.Id.Should().Be(new TemplateId(6));
		template.Name.Should().Be("Invoice");
		template.ExternalId.Should().BeNull();
		template.Folder.Should().Be("Default");
		template.CreatedAtUtc.Should().Be(Utc(2023, 8, 19, 11, 9, 21, 487));
		template.UpdatedAtUtc.Should().Be(Utc(2023, 8, 19, 11, 11, 47, 804));

		data.Values.Should().HaveCount(4);
		data.Values[0].Field.Should().Be("First Name");
		data.Values[0].Value!.Value.GetString().Should().Be("John");
		data.Values[3].Field.Should().Be("Signature");
		data.Values[3].Value!.Value.GetString().Should().Be("John Doe");

		var document = data.Documents.Should().ContainSingle().Subject;

		document.Name.Should().Be("sample-document");
		document.Url.Should().Be(new Uri("https://docuseal.com/blobs/proxy/eyJfcmFpbHMiOnsib/sample-document.pdf"));
	}

	[Fact]
	public void Parse_SubmissionCreated_BindsData() {
		var webhookEvent = (SubmissionWebhookEvent)DocuSealWebhook.Parse(Spec("submission-created"))!;

		webhookEvent.TimestampUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 518));

		var submission = webhookEvent.Data;

		submission.Id.Should().Be(new SubmissionId(1));
		submission.Name.Should().Be("Sample Submission");
		submission.Slug.Should().Be("VyL4szTwYoSvXq");
		submission.Source.Should().Be(SubmissionSource.Invite);
		submission.SubmittersOrder.Should().Be(SubmitterOrder.Random);
		submission.Status.Should().Be(SubmissionStatus.Pending);
		submission.AuditUrl.Should().BeNull();
		submission.CombinedDocumentUrl.Should().BeNull();
		submission.CompletedAtUtc.Should().BeNull();
		submission.CreatedAtUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 447));
		submission.Variables!["custom_variable"]!.GetValue<string>().Should().Be("value");
		submission.CreatedBy!.Email.Should().Be("john.doe@example.com");
		submission.Template!.Id.Should().Be(new TemplateId(1));
		submission.Template.Name.Should().Be("Sample Document");

		var submitter = submission.Submitters.Should().ContainSingle().Subject;

		submitter.Id.Should().Be(new SubmitterId(1));
		submitter.SubmissionId.Should().Be(new SubmissionId(1));
		submitter.Uuid.Should().Be(Guid.Parse("6b92a2d0-b511-4678-bccf-1e8a131f5030"));
		submitter.Email.Should().Be("mike@example.com");
		submitter.Status.Should().Be(SubmitterStatus.Awaiting);
		submitter.Role.Should().Be("First Party");
	}

	[Fact]
	public void Parse_SubmissionArchived_BindsData() {
		var webhookEvent = (SubmissionArchivedWebhookEvent)DocuSealWebhook.Parse(Spec("submission-archived"))!;

		webhookEvent.TimestampUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 518));
		webhookEvent.Data.Id.Should().Be(new SubmissionId(1));
		webhookEvent.Data.ArchivedAtUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 447));
	}

	[Fact]
	public void Parse_TemplateCreated_BindsData() {
		var webhookEvent = (TemplateWebhookEvent)DocuSealWebhook.Parse(Spec("template-created"))!;

		webhookEvent.TimestampUtc.Should().Be(Utc(2024, 5, 26, 16, 59, 47, 237));

		var template = webhookEvent.Data;

		template.Id.Should().Be(new TemplateId(1));
		template.Slug.Should().Be("UwRU9ir5dvhSRY");
		template.Name.Should().Be("Sample Document");
		template.Source.Should().Be(TemplateSource.Native);
		template.FolderId.Should().Be(new FolderId(1));
		template.Folder.Should().Be("Default");
		template.AuthorId.Should().Be(new UserId(1));
		template.Author.Email.Should().Be("john.doe@example.com");
		template.ArchivedAtUtc.Should().BeNull();
		template.HasSharedLink.Should().BeFalse();
		template.Schemas.Should().ContainSingle().Which.Name.Should().Be("sample-document");
		template.Fields.Should().HaveCount(2);
		template.Fields[0].Name.Should().Be("First Name");
		template.Fields[0].Type.Should().Be(FieldType.Text);
		template.Submitters.Should().ContainSingle().Which.Name.Should().Be("First Party");

		var document = template.Documents.Should().ContainSingle().Subject;

		document.Id.Should().Be(new DocumentId(12));
		document.Url.Should().Be(new Uri("https://docuseal.com/file/hash/sample-document.pdf"));
	}

	[Fact]
	public void Parse_TemplateArchived_BindsData() {
		var webhookEvent = (TemplateArchivedWebhookEvent)DocuSealWebhook.Parse(Spec("template-archived"))!;

		webhookEvent.TimestampUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 518));
		webhookEvent.Data.Id.Should().Be(new TemplateId(1));
		webhookEvent.Data.ArchivedAtUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 447));
	}

	//	============================================================================
	//	Unknown and Malformed
	//	============================================================================

	[Fact]
	public void Parse_UnknownEventType_ReturnsUnknownWithRawData() {
		const string json = """{ "event_type": "form.signed", "timestamp": "2024-05-26T17:32:33.518Z", "data": { "id": 7, "note": "kept" } }""";

		var webhookEvent = ParseBoth(json);

		var unknown = webhookEvent.Should().BeOfType<UnknownWebhookEvent>().Subject;

		unknown.Type.Should().Be(WebhookEventType.Unknown);
		unknown.RawType.Should().Be("form.signed");
		unknown.TimestampUtc.Should().Be(Utc(2024, 5, 26, 17, 32, 33, 518));
		unknown.Data.ValueKind.Should().Be(JsonValueKind.Object);
		unknown.Data.GetProperty("id").GetInt32().Should().Be(7);
		unknown.Data.GetProperty("note").GetString().Should().Be("kept");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("not json")]
	[InlineData("{")]
	[InlineData("null")]
	[InlineData("[]")]
	[InlineData("\"form.viewed\"")]
	[InlineData("{}")]
	[InlineData("""{ "timestamp": "2024-05-26T17:32:33.518Z", "data": {} }""")]
	[InlineData("""{ "event_type": 1, "timestamp": "2024-05-26T17:32:33.518Z", "data": {} }""")]
	[InlineData("""{ "event_type": "form.viewed", "data": {} }""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": 1716744753, "data": {} }""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": "yesterday", "data": {} }""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": "2024-05-26T17:32:33.518Z" }""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": "2024-05-26T17:32:33.518Z", "data": null }""")]
	[InlineData("""{ "event_type": "template.archived", "timestamp": "2024-05-26T17:32:33.518Z", "data": [] }""")]
	[InlineData("""{ "event_type": "submission.archived", "timestamp": "2024-05-26T17:32:33.518Z", "data": { "id": "one" } }""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": "2024-05-26T17:32:33.518Z", "data": { "created_at": "yesterday" } }""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": "2024-05-26T17:32:33.518Z", "data": {} } trailing""")]
	[InlineData("""{ "event_type": "form.viewed", "timestamp": "2024-05-26T17:32:33.518Z", "data": {} } {}""")]
	public void Parse_Malformed_ReturnsNullWithoutThrowing(
		string json) {
		var parse = () => ParseBoth(json);

		parse.Should().NotThrow().Which.Should().BeNull();
	}

	[Theory]
	[InlineData("submission.archived", typeof(SubmissionArchivedWebhookEvent))]
	[InlineData("template.archived", typeof(TemplateArchivedWebhookEvent))]
	public void Parse_ArchivedWithNullArchivedAt_BindsNullTimestamp(
		string eventType,
		Type subtype) {
		var json = $$"""{ "event_type": "{{eventType}}", "timestamp": "2024-05-26T17:32:33.518Z", "data": { "id": 1, "archived_at": null } }""";

		var webhookEvent = ParseBoth(json);

		webhookEvent.Should().NotBeNull();
		webhookEvent!.GetType().Should().Be(subtype);

		var archivedAtUtc = webhookEvent switch {
			SubmissionArchivedWebhookEvent e => e.Data.ArchivedAtUtc,
			TemplateArchivedWebhookEvent e => e.Data.ArchivedAtUtc,
			_ => throw new InvalidOperationException(subtype.Name)
		};

		archivedAtUtc.Should().BeNull();
	}

	[Theory]
	[InlineData("form.viewed")]
	[InlineData("form.signed")]
	public void Parse_LoneSurrogate_ReturnsNullWithoutThrowing(
		string eventType) {
		var json = "{ \"event_type\": \"" + eventType + "\uD800\", \"timestamp\": \"2024-05-26T17:32:33.518Z\", \"data\": {} }";
		var parse = () => DocuSealWebhook.Parse(json);

		parse.Should().NotThrow().Which.Should().BeNull();
	}

	[Theory]
	[InlineData("form.viewed")]
	[InlineData("form.signed")]
	public void Parse_InvalidUtf8_ReturnsNullWithoutThrowing(
		string eventType) {
		byte[] invalid = [0xED, 0xA0, 0x80, 0xFF];
		var prefix = Encoding.UTF8.GetBytes("{ \"event_type\": \"" + eventType);
		var suffix = Encoding.UTF8.GetBytes("\", \"timestamp\": \"2024-05-26T17:32:33.518Z\", \"data\": {} }");
		var bytes = prefix.Concat(invalid).Concat(suffix).ToArray();
		var parse = () => DocuSealWebhook.Parse(bytes);

		parse.Should().NotThrow().Which.Should().BeNull();
	}

	[Fact]
	public void Parse_NullString_ReturnsNull() => DocuSealWebhook.Parse((string)null!).Should().BeNull();

	[Fact]
	public void Parse_EmptySpan_ReturnsNull() => DocuSealWebhook.Parse(ReadOnlySpan<byte>.Empty).Should().BeNull();

	[Fact]
	public void Parse_ByteOrderMark_IsSkipped() {
		var json = Spec("template-archived");
		var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(json)).ToArray();

		DocuSealWebhook.Parse(bytes).Should().BeOfType<TemplateArchivedWebhookEvent>();
		DocuSealWebhook.Parse('\uFEFF' + json).Should().BeOfType<TemplateArchivedWebhookEvent>();
	}
}