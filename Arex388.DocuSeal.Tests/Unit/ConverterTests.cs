using FluentAssertions;
using System.Reflection;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

public sealed class ConverterTests {
	/// <summary>
	/// The serializer options the client actually uses. The converters are internal
	/// and only take effect through this registration, so the round-trips run
	/// against it rather than against hand-built options.
	/// </summary>
	private static readonly JsonSerializerOptions _options = (JsonSerializerOptions)typeof(IDocuSealClient).Assembly
		.GetType("Arex388.DocuSeal.DocuSealClient", throwOnError: true)!
		.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
		.Single(f => f.FieldType == typeof(JsonSerializerOptions))
		.GetValue(null)!;

	private static T Read<T>(
		string token) => JsonSerializer.Deserialize<T>($"\"{token}\"", _options)!;

	private static string? Write<T>(
		T value) => JsonSerializer.Deserialize<string?>(JsonSerializer.Serialize(value, _options));

	/// <summary>
	/// Reads and writes every token of an enum, asserts the table covers every
	/// non-<c>Unknown</c> member, and asserts the fallback and the null write.
	/// </summary>
	private static void AssertConverter<T>(
		params (string Token, T Member)[] cases)
		where T : struct, Enum {
		var unknown = Enum.Parse<T>("Unknown");

		foreach (var (token, member) in cases) {
			Read<T>(token).Should().Be(member, $"\"{token}\" must read to {typeof(T).Name}.{member}");
			Write(member).Should().Be(token, $"{typeof(T).Name}.{member} must write \"{token}\"");
		}

		cases.Select(c => c.Member).Should().BeEquivalentTo(
			Enum.GetValues<T>().Where(v => !v.Equals(unknown)),
			$"every {typeof(T).Name} member except Unknown needs a token");
		cases.Select(c => c.Token).Should().OnlyHaveUniqueItems();

		Read<T>("not_a_real_token").Should().Be(unknown);
		Read<T>("").Should().Be(unknown);
		Write(unknown).Should().BeNull($"{typeof(T).Name}.Unknown must not write a token");
	}

	//	============================================================================
	//	Currency
	//	============================================================================

	[Fact]
	public void Currency_EveryToken_RoundTrips() => AssertConverter(
		("AUD", Currency.Aud),
		("CAD", Currency.Cad),
		("CHF", Currency.Chf),
		("EUR", Currency.Eur),
		("GBP", Currency.Gbp),
		("SEK", Currency.Sek),
		("USD", Currency.Usd));

	//	============================================================================
	//	EventType
	//	============================================================================

	[Fact]
	public void EventType_EveryToken_RoundTrips() => AssertConverter(
		("api_complete_form", EventType.ApiCompletedForm),
		("bounce_email", EventType.BouncedEmail),
		("click_email", EventType.ClickedEmail),
		("click_sms", EventType.ClickedSms),
		("complete_form", EventType.CompletedForm),
		("complete_verification", EventType.CompletedVerification),
		("complaint_email", EventType.ComplaintEmail),
		("decline_form", EventType.DeclinedForm),
		("invite_party", EventType.InvitedParty),
		("open_email", EventType.OpenedEmail),
		("send_email", EventType.SentEmail),
		("send_reminder_email", EventType.SentReminderEmail),
		("send_sms", EventType.SentSms),
		("send_2fa_sms", EventType.SentTwoFactorSms),
		("start_form", EventType.StartedForm),
		("start_verification", EventType.StartedVerification),
		("phone_verified", EventType.VerifiedPhone),
		("view_form", EventType.ViewedForm));

	//	============================================================================
	//	FieldAlign
	//	============================================================================

	[Fact]
	public void FieldAlign_EveryToken_RoundTrips() => AssertConverter(
		("center", FieldAlign.Center),
		("left", FieldAlign.Left),
		("right", FieldAlign.Right));

	//	============================================================================
	//	FieldFont
	//	============================================================================

	[Fact]
	public void FieldFont_EveryToken_RoundTrips() => AssertConverter(
		("Courier", FieldFont.Courier),
		("Helvetica", FieldFont.Helvetica),
		("Times", FieldFont.Times));

	//	============================================================================
	//	FieldFontType
	//	============================================================================

	[Fact]
	public void FieldFontType_EveryToken_RoundTrips() => AssertConverter(
		("bold", FieldFontType.Bold),
		("bold_italic", FieldFontType.BoldItalic),
		("italic", FieldFontType.Italic));

	//	============================================================================
	//	FieldType
	//	============================================================================

	[Fact]
	public void FieldType_EveryToken_RoundTrips() => AssertConverter(
		("cells", FieldType.Cells),
		("checkbox", FieldType.Checkbox),
		("date", FieldType.Date),
		("file", FieldType.File),
		("heading", FieldType.Heading),
		("image", FieldType.Image),
		("initials", FieldType.Initials),
		("kba", FieldType.Kba),
		("multiple", FieldType.Multiple),
		("number", FieldType.Number),
		("payment", FieldType.Payment),
		("phone", FieldType.Phone),
		("radio", FieldType.Radio),
		("select", FieldType.Select),
		("signature", FieldType.Signature),
		("stamp", FieldType.Stamp),
		("strikethrough", FieldType.Strikethrough),
		("text", FieldType.Text),
		("verification", FieldType.Verification));

	//	============================================================================
	//	FieldVerticalAlign
	//	============================================================================

	[Fact]
	public void FieldVerticalAlign_EveryToken_RoundTrips() => AssertConverter(
		("bottom", FieldVerticalAlign.Bottom),
		("center", FieldVerticalAlign.Center),
		("top", FieldVerticalAlign.Top));

	//	============================================================================
	//	PageSize
	//	============================================================================

	[Fact]
	public void PageSize_EveryToken_RoundTrips() => AssertConverter(
		("A0", PageSize.A0),
		("A1", PageSize.A1),
		("A2", PageSize.A2),
		("A3", PageSize.A3),
		("A4", PageSize.A4),
		("A5", PageSize.A5),
		("A6", PageSize.A6),
		("Ledger", PageSize.Ledger),
		("Legal", PageSize.Legal),
		("Letter", PageSize.Letter),
		("Tabloid", PageSize.Tabloid));

	//	============================================================================
	//	SubmissionSource
	//	============================================================================

	[Fact]
	public void SubmissionSource_EveryToken_RoundTrips() => AssertConverter(
		("api", SubmissionSource.Api),
		("bulk", SubmissionSource.Bulk),
		("embed", SubmissionSource.Embed),
		("invite", SubmissionSource.Invite),
		("link", SubmissionSource.Link),
		("mcp", SubmissionSource.Mcp),
		("self", SubmissionSource.Self));

	//	============================================================================
	//	SubmissionStatus
	//	============================================================================

	[Fact]
	public void SubmissionStatus_EveryToken_RoundTrips() => AssertConverter(
		("completed", SubmissionStatus.Completed),
		("declined", SubmissionStatus.Declined),
		("expired", SubmissionStatus.Expired),
		("pending", SubmissionStatus.Pending));

	//	============================================================================
	//	SubmitterOrder
	//	============================================================================

	[Fact]
	public void SubmitterOrder_EveryToken_RoundTrips() => AssertConverter(
		("preserved", SubmitterOrder.Preserved),
		("random", SubmitterOrder.Random));

	//	============================================================================
	//	SubmitterStatus
	//	============================================================================

	[Fact]
	public void SubmitterStatus_EveryToken_RoundTrips() => AssertConverter(
		("awaiting", SubmitterStatus.Awaiting),
		("completed", SubmitterStatus.Completed),
		("declined", SubmitterStatus.Declined),
		("opened", SubmitterStatus.Opened),
		("pending", SubmitterStatus.Pending),
		("sent", SubmitterStatus.Sent));

	//	============================================================================
	//	TemplateSource
	//	============================================================================

	[Fact]
	public void TemplateSource_EveryToken_RoundTrips() => AssertConverter(
		("api", TemplateSource.Api),
		("embed", TemplateSource.Embed),
		("mcp", TemplateSource.Mcp),
		("native", TemplateSource.Native));

	//	============================================================================
	//	Fixtures
	//	============================================================================

	[Fact]
	public async Task GetTemplate_ParsesTemplateFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.GetTemplateAsync(ClientOperations.TemplateId);

		response.Success.Should().BeTrue();

		var template = response.Template!;

		template.Id.Should().Be(ClientOperations.TemplateId);
		template.Name.Should().Be("Test Template");
		template.Folder.Should().Be("Default");
		template.FolderId.Should().Be(new FolderId(7));
		template.ArchivedAtUtc.Should().BeNull();
		template.CreatedAtUtc.Should().Be(new DateTime(2024, 8, 5, 15, 21, 57, 375, DateTimeKind.Utc));
		template.Author.Email.Should().Be("author@example.com");
		template.Author.FirstName.Should().Be("Test");
		template.Schemas.Should().ContainSingle().Which.Name.Should().Be("test-document");
		template.Documents.Should().ContainSingle();
		template.Documents[0].Id.Should().Be(new DocumentId(5001));
		template.Documents[0].Name.Should().Be("test-document.pdf");
		template.Documents[0].AttachmentId.Should().Be(Guid.Parse("d94e615f-76e3-46d5-8f98-36bdacb8664a"));
		template.Fields.Should().HaveCount(3);
		template.Fields[0].Type.Should().Be(FieldType.Text);
		template.Fields[0].IsRequired.Should().BeTrue();
		template.Fields[0].Areas[0].Page.Should().Be(1);
		template.Fields[1].Type.Should().Be(FieldType.Signature);
		template.Fields[1].Areas[0].Width.Should().Be(.335M);
		template.Fields[2].Type.Should().Be(FieldType.Heading);
		template.Submitters.Should().ContainSingle().Which.Name.Should().Be("First Party");
	}

	[Fact]
	public async Task ListTemplates_ParsesTemplatesFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.ListTemplatesAsync();

		response.Success.Should().BeTrue();
		response.Pagination.Count.Should().Be(2);
		response.Templates.Should().HaveCount(2);
		response.Templates[1].Name.Should().Be("Archived Template");
		response.Templates[1].FolderId.Should().BeNull();
		response.Templates[1].ArchivedAtUtc.Should().Be(new DateTime(2024, 9, 1, 10, 0, 0, DateTimeKind.Utc));
		response.Templates[1].Fields[0].Type.Should().Be(FieldType.Date);
	}

	[Fact]
	public async Task GetSubmission_ParsesSubmissionFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.GetSubmissionAsync(ClientOperations.SubmissionId);

		response.Success.Should().BeTrue();

		var submission = response.Submission!;

		submission.Id.Should().Be(ClientOperations.SubmissionId, "the GET body carries the submission's id as `id`");
		submission.Status.Should().Be(SubmissionStatus.Completed);
		submission.SubmittersOrder.Should().Be(SubmitterOrder.Random);
		submission.CompletedAtUtc.Should().Be(new DateTime(2024, 8, 5, 16, 0, 0, DateTimeKind.Utc));
		submission.CreatedBy.LastName.Should().Be("Author");
		submission.Template.Id.Should().Be(ClientOperations.TemplateId);
		submission.Submitters.Should().HaveCount(2);
		submission.Submitters[0].Id.Should().Be(ClientOperations.SubmitterId);
		submission.Submitters[0].SubmissionId.Should().Be(ClientOperations.SubmissionId);
		submission.Submitters[0].Status.Should().Be(SubmitterStatus.Completed);
		submission.Submitters[1].Status.Should().Be(SubmitterStatus.Sent);
		submission.Submitters[1].OpenedAtUtc.Should().BeNull();
		submission.Events.Select(e => e.Type).Should().Equal(
			EventType.SentEmail,
			EventType.ClickedEmail,
			EventType.ViewedForm,
			EventType.StartedForm,
			EventType.CompletedForm,
			EventType.DeclinedForm,
			EventType.Unknown);
		submission.Events[0].SubmitterId.Should().Be(ClientOperations.SubmitterId);
		submission.Events[0].Id.Should().Be(new EventId(4001));
	}

	[Fact]
	public async Task ListSubmissions_ParsesSubmissionsFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.ListSubmissionsAsync();

		response.Success.Should().BeTrue();
		response.Pagination.Count.Should().Be(2);
		response.Submissions.Should().HaveCount(2);
		response.Submissions.Select(s => s.Id).Should().Equal(ClientOperations.SubmissionId, new SubmissionId(2002));
		response.Submissions[1].Status.Should().Be(SubmissionStatus.Pending);
		response.Submissions[1].SubmittersOrder.Should().Be(SubmitterOrder.Preserved);
		response.Submissions[1].Submitters[0].Status.Should().Be(SubmitterStatus.Awaiting);
		response.Submissions[1].Template.Name.Should().Be("Archived Template");
	}

	[Fact]
	public async Task CreateSubmission_ParsesCreatedFixture_IdIsTheSubmissionId() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.CreateSubmissionAsync(new CreateSubmission.Request {
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = "signer1@example.com"
				}
			],
			TemplateId = ClientOperations.TemplateId
		});

		response.Success.Should().BeTrue();

		//	The created fixture's first object has `id` 3001 (the submitter) and
		//	`submission_id` 2001 (the submission); Id must be the latter.
		response.Submission!.Id.Should().Be(ClientOperations.SubmissionId);
		response.Submission.Id.Should().NotBe(new SubmissionId(ClientOperations.SubmitterId.Value));
	}

	[Theory]
	[InlineData("""{ "id": 2001 }""", 2001)]
	[InlineData("""{ "id": 3001, "submission_id": 2001 }""", 2001)]
	[InlineData("""{ "submission_id": 2001, "id": 3001 }""", 2001)]
	[InlineData("""{ "submission_id": 2001 }""", 2001)]
	[InlineData("""{ "id": 2001, "submission_id": null }""", 2001)]
	public void Submission_Id_PrefersSubmissionId_OverId(
		string json,
		int expected) => JsonSerializer.Deserialize<Submission>(json, _options)!.Id.Should().Be(new SubmissionId(expected));

	[Theory]
	[InlineData("[]")]
	[InlineData("2001")]
	public void Submission_NonObject_Throws(
		string json) {
		var read = () => JsonSerializer.Deserialize<Submission>(json, _options);

		read.Should().Throw<JsonException>();
	}

	[Fact]
	public void Submission_Write_EmitsId() {
		var json = JsonSerializer.Serialize(JsonSerializer.Deserialize<Submission>("""{ "id": 2001 }""", _options), _options);

		using var document = JsonDocument.Parse(json);

		document.RootElement.GetProperty("id").GetInt32().Should().Be(2001);
		document.RootElement.TryGetProperty("submission_id", out _).Should().BeFalse();
	}

	[Fact]
	public async Task GetSubmitter_ParsesSubmitterFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.GetSubmitterAsync(ClientOperations.SubmitterId);

		response.Success.Should().BeTrue();

		var submitter = response.Submitter!;

		submitter.Id.Should().Be(ClientOperations.SubmitterId);
		submitter.SubmissionId.Should().Be(ClientOperations.SubmissionId);
		submitter.Email.Should().Be("signer1@example.com");
		submitter.Name.Should().Be("Signer One");
		submitter.Phone.Should().Be("+15555550101");
		submitter.Role.Should().Be("First Party");
		submitter.Status.Should().Be(SubmitterStatus.Completed);
		submitter.SentAtUtc.Should().Be(new DateTime(2024, 8, 5, 15, 30, 5, DateTimeKind.Utc));
	}

	[Fact]
	public async Task ListSubmitters_ParsesSubmittersFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.ListSubmittersAsync();

		response.Success.Should().BeTrue();
		response.Pagination.Count.Should().Be(2);
		response.Submitters.Should().HaveCount(2);
		response.Submitters[1].Status.Should().Be(SubmitterStatus.Opened);
		response.Submitters[1].Name.Should().BeNull();
		response.Submitters[1].CompletedAtUtc.Should().BeNull();
	}
}
