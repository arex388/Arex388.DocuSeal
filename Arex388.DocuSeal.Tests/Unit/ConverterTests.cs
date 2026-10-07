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

	//	============================================================================
	//	EventType
	//	============================================================================

	[Theory]
	[InlineData("click_email", EventType.OpenedEmail)]
	[InlineData("complete_form", EventType.CompletedForm)]
	[InlineData("start_form", EventType.StartedForm)]
	[InlineData("view_form", EventType.ViewedForm)]
	public void EventType_KnownToken_RoundTrips(
		string token,
		EventType expected) {
		Read<EventType>(token).Should().Be(expected);
		Write(expected).Should().Be(token);
	}

	[Fact]
	public void EventType_SentEmail_RoundTrips() {
		Read<EventType>("send_email").Should().Be(EventType.SentEmail);
		Write(EventType.SentEmail).Should().Be("send_email");
	}

	[Theory]
	[InlineData("decline_form")]
	[InlineData("")]
	public void EventType_UnknownToken_FallsBackToUnknown(
		string token) => Read<EventType>(token).Should().Be(EventType.Unknown);

	//	============================================================================
	//	FieldType
	//	============================================================================

	[Theory]
	[InlineData("cells", FieldType.Cells)]
	[InlineData("checkbox", FieldType.Checkbox)]
	[InlineData("date", FieldType.Date)]
	[InlineData("file", FieldType.File)]
	[InlineData("image", FieldType.Image)]
	[InlineData("initials", FieldType.Initials)]
	[InlineData("multiple", FieldType.Multiple)]
	[InlineData("payment", FieldType.Payment)]
	[InlineData("phone", FieldType.Phone)]
	[InlineData("radio", FieldType.Radio)]
	[InlineData("select", FieldType.Select)]
	[InlineData("signature", FieldType.Signature)]
	[InlineData("stamp", FieldType.Stamp)]
	[InlineData("text", FieldType.Text)]
	public void FieldType_KnownToken_RoundTrips(
		string token,
		FieldType expected) {
		Read<FieldType>(token).Should().Be(expected);
		Write(expected).Should().Be(token);
	}

	[Theory]
	[InlineData("heading")]
	[InlineData("verification")]
	[InlineData("")]
	public void FieldType_UnknownToken_FallsBackToText(
		string token) => Read<FieldType>(token).Should().Be(FieldType.Text);

	//	============================================================================
	//	SubmitterOrder
	//	============================================================================

	[Theory]
	[InlineData("preserved", SubmitterOrder.Preserved)]
	[InlineData("random", SubmitterOrder.Random)]
	public void SubmitterOrder_KnownToken_RoundTrips(
		string token,
		SubmitterOrder expected) {
		Read<SubmitterOrder>(token).Should().Be(expected);
		Write(expected).Should().Be(token);
	}

	[Theory]
	[InlineData("sequential")]
	[InlineData("")]
	public void SubmitterOrder_UnknownToken_FallsBackToUnknown(
		string token) => Read<SubmitterOrder>(token).Should().Be(SubmitterOrder.Unknown);

	//	============================================================================
	//	SubmitterStatus
	//	============================================================================

	[Theory]
	[InlineData("completed", SubmitterStatus.Completed)]
	[InlineData("opened", SubmitterStatus.Opened)]
	[InlineData("pending", SubmitterStatus.Pending)]
	[InlineData("sent", SubmitterStatus.Sent)]
	public void SubmitterStatus_KnownToken_RoundTrips(
		string token,
		SubmitterStatus expected) {
		Read<SubmitterStatus>(token).Should().Be(expected);
		Write(expected).Should().Be(token);
	}

	[Theory]
	[InlineData("awaiting")]
	[InlineData("declined")]
	[InlineData("")]
	public void SubmitterStatus_UnknownToken_FallsBackToUnknown(
		string token) => Read<SubmitterStatus>(token).Should().Be(SubmitterStatus.Unknown);

	//	============================================================================
	//	Every member has a token
	//	============================================================================

	[Fact]
	public void EveryKnownMember_WritesAToken() {
		foreach (var value in Enum.GetValues<EventType>().Where(v => v != EventType.Unknown)) {
			Write(value).Should().NotBeNullOrEmpty($"EventType.{value} must have a wire token");
		}

		foreach (var value in Enum.GetValues<FieldType>()) {
			Write(value).Should().NotBeNullOrEmpty($"FieldType.{value} must have a wire token");
		}

		foreach (var value in Enum.GetValues<SubmitterOrder>().Where(v => v != SubmitterOrder.Unknown)) {
			Write(value).Should().NotBeNullOrEmpty($"SubmitterOrder.{value} must have a wire token");
		}

		foreach (var value in Enum.GetValues<SubmitterStatus>().Where(v => v != SubmitterStatus.Unknown)) {
			Write(value).Should().NotBeNullOrEmpty($"SubmitterStatus.{value} must have a wire token");
		}
	}

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
		template.Fields[2].Type.Should().Be(FieldType.Text, "an unknown field type falls back to Text");
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

		submission.Status.Should().Be(SubmitterStatus.Completed);
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
			EventType.OpenedEmail,
			EventType.ViewedForm,
			EventType.StartedForm,
			EventType.CompletedForm,
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
		response.Submissions[1].Status.Should().Be(SubmitterStatus.Pending);
		response.Submissions[1].SubmittersOrder.Should().Be(SubmitterOrder.Preserved);
		response.Submissions[1].Submitters[0].Status.Should().Be(SubmitterStatus.Unknown, "awaiting has no SubmitterStatus member");
		response.Submissions[1].Template.Name.Should().Be("Archived Template");
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
