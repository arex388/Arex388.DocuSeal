using FluentAssertions;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Binds the response models against the OpenAPI spec's example payloads
/// (<c>TestData/Spec</c>, one file per documented 200 response) and against
/// inline payloads for the members the examples leave empty.
/// </summary>
public sealed class ModelTests {
	private static readonly JsonSerializerOptions _options = TestClients.JsonOptions;

	private static readonly string _specDirectory = Path.Combine(AppContext.BaseDirectory, "Spec");

	private static string Spec(
		string name) => File.ReadAllText(Path.Combine(_specDirectory, $"{name}.json"));

	private static DateTime Utc(
		int year,
		int month,
		int day,
		int hour,
		int minute,
		int second,
		int millisecond) => new(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);

	private static T Read<T>(
		string json) => JsonSerializer.Deserialize<T>(json, _options)!;

	//	============================================================================
	//	Template
	//	============================================================================

	[Fact]
	public async Task Template_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("template"), out _);

		var response = await docuSeal.GetTemplateAsync(new TemplateId(1));

		response.Success.Should().BeTrue();

		var template = response.Template!;

		template.Id.Should().Be(new TemplateId(1));
		template.Slug.Should().Be("iRgjDX7WDK6BRo");
		template.Name.Should().Be("Example Template");
		template.Preferences.Should().NotBeNull();
		template.Preferences!.Count.Should().Be(0);
		template.VariablesSchema.Should().BeNull("the spec example omits variables_schema");
		template.Schemas.Should().ContainSingle().Which.Name.Should().Be("example-document");
		template.AuthorId.Should().Be(new UserId(1));
		template.ArchivedAtUtc.Should().BeNull();
		template.CreatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 21, 57, 375));
		template.UpdatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 22, 55, 94));
		template.Source.Should().Be(TemplateSource.Native);
		template.ExternalId.Should().Be("c248ffba-ef81-48b7-8e17-e3cecda1c1c5");
		template.FolderId.Should().Be(new FolderId(1));
		template.Folder.Should().Be("Default");
		template.HasSharedLink.Should().BeTrue();
		template.Author.Id.Should().Be(new UserId(1));
		template.Author.FirstName.Should().Be("John");
		template.Author.LastName.Should().Be("Doe");
		template.Author.Email.Should().Be("john.doe@example.com");

		var document = template.Documents.Should().ContainSingle().Subject;

		document.Id.Should().Be(new DocumentId(5));
		document.AttachmentId.Should().Be(Guid.Parse("d94e615f-76e3-46d5-8f98-36bdacb8664a"));
		document.Url.Should().Be(new Uri("https://docuseal.com/file/hash/sample-document.pdf"));
		document.PreviewUri.Should().Be(new Uri("https://docuseal.com/file/hash/0.jpg"));
		document.Name.Should().Be("example-document.pdf");

		var field = template.Fields.Should().ContainSingle().Subject;

		field.Id.Should().Be(Guid.Parse("594bdf04-d941-4ca6-aa73-93e61d625c02"));
		field.Name.Should().Be("Full Name");
		field.Type.Should().Be(FieldType.Text);
		field.IsRequired.Should().BeTrue();
		field.Preferences.Should().NotBeNull("the spec example sends an empty preferences object");
		field.Areas.Should().ContainSingle().Which.Height.Should().Be(0.04616895874263263M);
	}

	[Fact]
	public async Task Template_SubmitterUuid_JoinsToTheFieldsSubmitterId() {
		var docuSeal = TestClients.CreateWithJson(Spec("template"), out _);

		var template = (await docuSeal.GetTemplateAsync(new TemplateId(1))).Template!;

		var submitter = template.Submitters.Should().ContainSingle().Subject;

		submitter.Name.Should().Be("First Party");
		submitter.Uuid.Should().Be(Guid.Parse("0954d146-db8c-4772-aafe-2effc7c0e0c0"));
		template.Fields.Should().OnlyContain(f => f.SubmitterId == submitter.Uuid);
	}

	[Fact]
	public async Task TemplateList_BindsSpecExample_AndPagination() {
		var docuSeal = TestClients.CreateWithJson(Spec("template-list"), out _);

		var response = await docuSeal.ListTemplatesAsync();

		response.Success.Should().BeTrue();
		response.Pagination.Count.Should().Be(1);
		response.Pagination.Next.Should().Be(1);
		response.Pagination.Previous.Should().Be(2);

		var template = response.Templates.Should().ContainSingle().Subject;

		template.Id.Should().Be(new TemplateId(1));
		template.Slug.Should().Be("iRgjDX7WDK6BRo");
		template.Source.Should().Be(TemplateSource.Native);
		template.HasSharedLink.Should().BeTrue();
		template.AuthorId.Should().Be(new UserId(1));
		template.Submitters.Should().ContainSingle().Which.Uuid.Should().Be(template.Fields[0].SubmitterId);
	}

	[Fact]
	public void Template_FreeFormObjects_BindAsJsonObjects() {
		var template = Read<Template>("""
			{
				"preferences": { "bcc_completed": "audit@example.com" },
				"variables_schema": { "name": { "type": "string" } }
			}
			""");

		template.Preferences!["bcc_completed"]!.GetValue<string>().Should().Be("audit@example.com");
		template.VariablesSchema!["name"]!["type"]!.GetValue<string>().Should().Be("string");
	}

	//	============================================================================
	//	Submission
	//	============================================================================

	[Fact]
	public async Task Submission_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("submission"), out _);

		var response = await docuSeal.GetSubmissionAsync(new SubmissionId(1));

		response.Success.Should().BeTrue();

		var submission = response.Submission!;

		submission.Id.Should().Be(new SubmissionId(1));
		submission.Name.Should().BeNull();
		submission.Source.Should().Be(SubmissionSource.Link);
		submission.SubmittersOrder.Should().Be(SubmitterOrder.Random);
		submission.Slug.Should().Be("VyL4szTwYoSvXq");
		submission.Status.Should().Be(SubmissionStatus.Completed);
		submission.AuditUrl.Should().Be(new Uri("https://docuseal.com/blobs/proxy/hash/example.pdf"));
		submission.CombinedDocumentUrl.Should().BeNull();
		submission.ExpireAtUtc.Should().BeNull();
		submission.Variables.Should().NotBeNull();
		submission.Variables!.Count.Should().Be(0);
		submission.CompletedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 49, 21, 701));
		submission.CreatedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 48, 17, 166));
		submission.UpdatedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 49, 21, 895));
		submission.ArchivedAtUtc.Should().BeNull();
		submission.CreatedBy!.Id.Should().Be(new UserId(1));
		submission.CreatedBy.FirstName.Should().Be("Bob");
		submission.CreatedBy.LastName.Should().Be("Smith");
		submission.CreatedBy.Email.Should().Be("bob.smith@example.com");

		var document = submission.Documents.Should().ContainSingle().Subject;

		document.Name.Should().Be("example");
		document.Url.Should().Be(new Uri("https://docuseal.com/file/hash/example.pdf"));

		var template = submission.Template!;

		template.Id.Should().Be(new TemplateId(1));
		template.Name.Should().Be("Example Template");
		template.ExternalId.Should().Be("Temp123");
		template.Folder.Should().Be("Default");
		template.CreatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
		template.UpdatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));

		var @event = submission.Events.Should().ContainSingle().Subject;

		@event.Id.Should().Be(new EventId(1));
		@event.SubmitterId.Should().Be(new SubmitterId(2));
		@event.Type.Should().Be(EventType.ViewedForm);
		@event.OccurredAtUtc.Should().Be(Utc(2023, 12, 14, 15, 47, 24, 566));
		@event.Data.Should().NotBeNull();
		@event.Data!.Count.Should().Be(0);

		var submitter = submission.Submitters.Should().ContainSingle().Subject;

		submitter.Id.Should().Be(new SubmitterId(1));
		submitter.SubmissionId.Should().Be(new SubmissionId(1));
		submitter.Uuid.Should().Be(Guid.Parse("0954d146-db8c-4772-aafe-2effc7c0e0c0"));
		submitter.Slug.Should().Be("dsEeWrhRD8yDXT");
		submitter.Email.Should().Be("submitter@example.com");
		submitter.SentAtUtc.Should().Be(Utc(2023, 12, 14, 15, 45, 49, 11));
		submitter.OpenedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 48, 23, 11));
		submitter.CompletedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 49, 21, 701));
		submitter.DeclinedAtUtc.Should().BeNull();
		submitter.Name.Should().Be("John Doe");
		submitter.Phone.Should().Be("+1234567890");
		submitter.ExternalId.Should().BeNull();
		submitter.Status.Should().Be(SubmitterStatus.Completed);
		submitter.Role.Should().Be("First Party");
		submitter.Metadata.Should().NotBeNull();
		submitter.Values.Should().ContainSingle().Which.Field.Should().Be("Full Name");
		submitter.Values[0].Value!.Value.GetString().Should().Be("John Doe");
		submitter.Documents.Should().ContainSingle().Which.Url.Should().Be(new Uri("https://docuseal.com/blobs/proxy/hash/example.pdf"));
	}

	[Fact]
	public async Task SubmissionList_BindsSpecExample_AndPagination() {
		var docuSeal = TestClients.CreateWithJson(Spec("submission-list"), out _);

		var response = await docuSeal.ListSubmissionsAsync();

		response.Success.Should().BeTrue();
		response.Pagination.Count.Should().Be(1);
		response.Pagination.Next.Should().Be(1);
		response.Pagination.Previous.Should().Be(1);

		var submission = response.Submissions.Should().ContainSingle().Subject;

		submission.Id.Should().Be(new SubmissionId(1));
		submission.Source.Should().Be(SubmissionSource.Link);
		submission.Slug.Should().Be("VyL4szTwYoSvXq");
		submission.Status.Should().Be(SubmissionStatus.Completed);
		submission.SubmittersOrder.Should().Be(SubmitterOrder.Random);
		submission.AuditUrl.Should().Be(new Uri("https://docuseal.com/file/hash/example.pdf"));
		submission.CombinedDocumentUrl.Should().BeNull();
		submission.ExpireAtUtc.Should().BeNull();
		submission.Variables.Should().NotBeNull();
		submission.CompletedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 49, 21, 895));
		submission.Template!.ExternalId.Should().Be("Temp123");
		submission.CreatedBy!.Email.Should().Be("bob.smith@example.com");
		submission.Events.Should().BeEmpty("the list item shape carries no events");
		submission.Documents.Should().BeEmpty("the list item shape carries no documents");

		var submitter = submission.Submitters.Should().ContainSingle().Subject;

		submitter.Uuid.Should().Be(Guid.Parse("0954d146-db8c-4772-aafe-2effc7c0e0c0"));
		submitter.Slug.Should().Be("dsEeWrhRD8yDXT");
		submitter.Metadata.Should().NotBeNull();
		submitter.Preferences.Should().NotBeNull();
		submitter.Values.Should().BeEmpty();
	}

	[Fact]
	public void Submission_NullableMembers_BindNull() {
		var submission = Read<Submission>("""
			{
				"id": 1,
				"audit_log_url": null,
				"combined_document_url": null,
				"expire_at": "2024-09-01T12:00:00.000Z",
				"created_by_user": null,
				"variables": { "name": "Acme" }
			}
			""");

		submission.AuditUrl.Should().BeNull();
		submission.CombinedDocumentUrl.Should().BeNull();
		submission.CreatedBy.Should().BeNull();
		submission.Template.Should().BeNull();
		submission.ExpireAtUtc.Should().Be(new DateTime(2024, 9, 1, 12, 0, 0, DateTimeKind.Utc));
		submission.Variables!["name"]!.GetValue<string>().Should().Be("Acme");
	}

	//	============================================================================
	//	Submitter
	//	============================================================================

	[Fact]
	public async Task Submitter_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("submitter"), out _);

		var response = await docuSeal.GetSubmitterAsync(new SubmitterId(7));

		response.Success.Should().BeTrue();

		var submitter = response.Submitter!;

		submitter.Id.Should().Be(new SubmitterId(7));
		submitter.SubmissionId.Should().Be(new SubmissionId(3));
		submitter.Uuid.Should().Be(Guid.Parse("0954d146-db8c-4772-aafe-2effc7c0e0c0"));
		submitter.Email.Should().Be("submitter@example.com");
		submitter.Slug.Should().Be("dsEeWrhRD8yDXT");
		submitter.SentAtUtc.Should().Be(Utc(2023, 12, 14, 15, 45, 49, 11));
		submitter.OpenedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 48, 23, 11));
		submitter.CompletedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 49, 21, 701));
		submitter.DeclinedAtUtc.Should().BeNull();
		submitter.CreatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 48, 17, 173));
		submitter.UpdatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
		submitter.Name.Should().Be("John Doe");
		submitter.Phone.Should().Be("+1234567890");
		submitter.Status.Should().Be(SubmitterStatus.Completed);
		submitter.ExternalId.Should().BeNull();
		submitter.Metadata.Should().NotBeNull();
		submitter.Preferences.Should().NotBeNull();
		submitter.Role.Should().Be("First Party");

		var template = submitter.Template!;

		template.Id.Should().Be(new TemplateId(2));
		template.Name.Should().Be("Example Template");
		template.CreatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
		template.UpdatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));

		var @event = submitter.Events.Should().ContainSingle().Subject;

		@event.Id.Should().Be(new EventId(12));
		@event.SubmitterId.Should().Be(new SubmitterId(7));
		@event.Type.Should().Be(EventType.ViewedForm);
		@event.OccurredAtUtc.Should().Be(Utc(2023, 12, 14, 15, 47, 17, 351));
		@event.Data.Should().NotBeNull();

		submitter.Values.Should().ContainSingle().Which.Field.Should().Be("Full Name");
		submitter.Values[0].Value!.Value.GetString().Should().Be("John Doe");
		submitter.Documents.Should().ContainSingle().Which.Name.Should().Be("sample-document");
		submitter.Documents[0].Url.Should().Be(new Uri("https://docuseal.com/file/hash/sample-document.pdf"));
	}

	[Fact]
	public async Task SubmitterList_BindsSpecExample_AndPagination() {
		var docuSeal = TestClients.CreateWithJson(Spec("submitter-list"), out _);

		var response = await docuSeal.ListSubmittersAsync();

		response.Success.Should().BeTrue();
		response.Pagination.Count.Should().Be(1);
		response.Pagination.Next.Should().Be(1);
		response.Pagination.Previous.Should().Be(1);

		var submitter = response.Submitters.Should().ContainSingle().Subject;

		submitter.Id.Should().Be(new SubmitterId(7));
		submitter.SubmissionId.Should().Be(new SubmissionId(3));
		submitter.Uuid.Should().Be(Guid.Parse("0954d146-db8c-4772-aafe-2effc7c0e0c0"));
		submitter.Slug.Should().Be("dsEeWrhRD8yDXT");
		submitter.Template!.Id.Should().Be(new TemplateId(2));
		submitter.Events.Should().ContainSingle().Which.OccurredAtUtc.Should().Be(Utc(2023, 12, 14, 15, 48, 17, 351));
		submitter.Values.Should().ContainSingle();
		submitter.Documents.Should().ContainSingle();
	}

	[Fact]
	public void Submitter_NullableMembers_BindNull() {
		var submitter = Read<Submitter>("""
			{
				"id": 1,
				"email": null,
				"external_id": null,
				"declined_at": "2024-08-05T16:05:00.000Z",
				"template": null
			}
			""");

		submitter.Email.Should().BeNull();
		submitter.ExternalId.Should().BeNull();
		submitter.Template.Should().BeNull();
		submitter.DeclinedAtUtc.Should().Be(new DateTime(2024, 8, 5, 16, 5, 0, DateTimeKind.Utc));
	}

	//	============================================================================
	//	FieldValue
	//	============================================================================

	[Theory]
	[InlineData("""{ "field": "Name", "value": "John" }""", JsonValueKind.String)]
	[InlineData("""{ "field": "Age", "value": 42 }""", JsonValueKind.Number)]
	[InlineData("""{ "field": "Price", "value": 4.5 }""", JsonValueKind.Number)]
	[InlineData("""{ "field": "Agree", "value": true }""", JsonValueKind.True)]
	[InlineData("""{ "field": "Choices", "value": ["a", "b"] }""", JsonValueKind.Array)]
	public void FieldValue_Value_BindsEveryJsonShape(
		string json,
		JsonValueKind kind) {
		var value = Read<FieldValue>(json);

		value.Field.Should().NotBeNullOrEmpty();
		value.Value.Should().NotBeNull();
		value.Value!.Value.ValueKind.Should().Be(kind);
	}

	[Fact]
	public void FieldValue_Value_BindsNullAsNull() {
		var value = Read<FieldValue>("""{ "field": "Name", "value": null }""");

		value.Field.Should().Be("Name");
		value.Value.Should().BeNull();
	}

	//	============================================================================
	//	Field
	//	============================================================================

	[Fact]
	public void FieldPreferences_BindsEveryMember() {
		var field = Read<Field>("""
			{
				"uuid": "594bdf04-d941-4ca6-aa73-93e61d625c02",
				"preferences": {
					"font_size": 12,
					"font_type": "bold_italic",
					"font": "Courier",
					"color": "#ff0000",
					"background": "#00ff00",
					"align": "center",
					"valign": "bottom",
					"format": "DD/MM/YYYY",
					"price": 19.99,
					"currency": "EUR",
					"mask": true,
					"reasons": ["I approve", "I agree"]
				}
			}
			""");

		var preferences = field.Preferences!;

		preferences.FontSize.Should().Be(12);
		preferences.FontType.Should().Be(FieldFontType.BoldItalic);
		preferences.Font.Should().Be(FieldFont.Courier);
		preferences.Color.Should().Be("#ff0000");
		preferences.Background.Should().Be("#00ff00");
		preferences.Align.Should().Be(FieldAlign.Center);
		preferences.VerticalAlign.Should().Be(FieldVerticalAlign.Bottom);
		preferences.Format.Should().Be("DD/MM/YYYY");
		preferences.Price.Should().Be(19.99M);
		preferences.Currency.Should().Be(Currency.Eur);
		preferences.Mask!.Value.GetBoolean().Should().BeTrue();
		preferences.Reasons.Should().Equal("I approve", "I agree");
	}

	[Theory]
	[InlineData("true", JsonValueKind.True)]
	[InlineData("false", JsonValueKind.False)]
	[InlineData("4", JsonValueKind.Number)]
	public void FieldPreferences_Mask_BindsBooleanOrInteger(
		string mask,
		JsonValueKind kind) {
		var preferences = Read<FieldPreferences>($$"""{ "mask": {{mask}} }""");

		preferences.Mask.Should().NotBeNull();
		preferences.Mask!.Value.ValueKind.Should().Be(kind);
	}

	[Fact]
	public void FieldPreferences_NullMembers_BindNull() {
		var preferences = Read<FieldPreferences>("""
			{
				"font_size": null,
				"font_type": null,
				"font": null,
				"color": null,
				"background": null,
				"align": null,
				"valign": null,
				"format": null,
				"price": null,
				"currency": null,
				"mask": null,
				"reasons": null
			}
			""");

		preferences.Should().BeEquivalentTo(new FieldPreferences());
	}

	//	============================================================================
	//	Event
	//	============================================================================

	[Fact]
	public void Event_Data_BindsAsAFreeFormObject() {
		var @event = Read<Event>("""
			{
				"id": 1,
				"submitter_id": 2,
				"event_type": "send_email",
				"event_timestamp": "2024-08-05T15:30:05.000Z",
				"data": { "reason": "I do not agree", "nested": { "deep": [1, 2, 3] } }
			}
			""");

		@event.OccurredAtUtc.Should().Be(new DateTime(2024, 8, 5, 15, 30, 5, DateTimeKind.Utc));
		@event.Data!["reason"]!.GetValue<string>().Should().Be("I do not agree");
		@event.Data["nested"]!["deep"]!.AsArray().Should().HaveCount(3);
	}

	//	============================================================================
	//	User
	//	============================================================================

	[Fact]
	public void User_Names_AreNullable() {
		var user = Read<User>("""{ "id": 1, "first_name": null, "last_name": null, "email": "user@example.com" }""");

		user.FirstName.Should().BeNull();
		user.LastName.Should().BeNull();
		user.Email.Should().Be("user@example.com");
	}

	//	============================================================================
	//	ResponsePagination
	//	============================================================================

	[Fact]
	public void ResponsePagination_NextAndPrev_BindNullable() {
		var pagination = Read<ResponsePagination>("""{ "count": 10, "next": 42, "prev": null }""");

		pagination.Count.Should().Be(10);
		pagination.Next.Should().Be(42);
		pagination.Previous.Should().BeNull();
	}
}