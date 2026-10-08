using FluentAssertions;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Binds the create, update, and archive responses against the OpenAPI spec's
/// example payloads (<c>TestData/Spec</c>) and against the shared mock fixtures.
/// </summary>
public sealed class ResponsePayloadTests {
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

	private static CreateSubmission.Request CreateSubmissionRequest() => new() {
		Submitters = [
			new CreateSubmission.RequestSubmitter {
				Email = "signer1@example.com"
			},
			new CreateSubmission.RequestSubmitter {
				Email = "signer2@example.com"
			}
		],
		TemplateId = ClientOperations.TemplateId
	};

	//	============================================================================
	//	CreateSubmission
	//	============================================================================

	[Fact]
	public async Task CreateSubmission_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("submission-create"), out _);

		var response = await docuSeal.CreateSubmissionAsync(CreateSubmissionRequest());

		response.Success.Should().BeTrue();
		response.SubmissionId.Should().Be(new SubmissionId(1));

		var submitter = response.Submitters.Should().ContainSingle().Subject;

		submitter.Id.Should().Be(new SubmitterId(1));
		submitter.SubmissionId.Should().Be(new SubmissionId(1));
		submitter.Uuid.Should().Be(Guid.Parse("884d545b-3396-49f1-8c07-05b8b2a78755"));
		submitter.Email.Should().Be("john.doe@example.com");
		submitter.Slug.Should().Be("pAMimKcyrLjqVt");
		submitter.SentAtUtc.Should().Be(Utc(2023, 12, 13, 23, 4, 4, 252));
		submitter.OpenedAtUtc.Should().BeNull();
		submitter.CompletedAtUtc.Should().BeNull();
		submitter.DeclinedAtUtc.Should().BeNull();
		submitter.CreatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
		submitter.UpdatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
		submitter.Name.Should().Be("string");
		submitter.Phone.Should().Be("+1234567890");
		submitter.ExternalId.Should().Be("2321");
		submitter.Metadata!["customData"]!.GetValue<string>().Should().Be("custom value");
		submitter.Status.Should().Be(SubmitterStatus.Sent);
		submitter.Values.Should().ContainSingle().Which.Field.Should().Be("Full Name");
		submitter.Preferences!["send_email"]!.GetValue<bool>().Should().BeTrue();
		submitter.Preferences["send_sms"]!.GetValue<bool>().Should().BeFalse();
		submitter.Role.Should().Be("First Party");
		submitter.EmbedSrc.Should().Be(new Uri("https://docuseal.com/s/pAMimKcyrLjqVt"));
	}

	[Fact]
	public async Task CreateSubmission_TwoSubmitters_ReturnsBothAndTheSharedSubmissionId() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.CreateSubmissionAsync(CreateSubmissionRequest());

		response.Success.Should().BeTrue();

		//	Each element's `id` is the submitter and `submission_id` is the submission.
		response.SubmissionId.Should().Be(ClientOperations.SubmissionId);
		response.Submitters.Should().HaveCount(2);
		response.Submitters.Select(s => s.Id).Should().Equal(ClientOperations.SubmitterId, new SubmitterId(3002));
		response.Submitters.Select(s => s.SubmissionId).Should().AllBeEquivalentTo(ClientOperations.SubmissionId);
		response.Submitters.Select(s => s.Email).Should().Equal("signer1@example.com", "signer2@example.com");
		response.Submitters.Select(s => s.Role).Should().Equal("First Party", "Second Party");
		response.Submitters.Select(s => s.Status).Should().Equal(SubmitterStatus.Sent, SubmitterStatus.Awaiting);
		response.Submitters.Select(s => s.EmbedSrc).Should().Equal(
			new Uri("https://docuseal.example.com/s/dsEeWrhRD8yDXT"),
			new Uri("https://docuseal.example.com/s/eTfFxsiSE9zEYU"));
	}

	[Fact]
	public async Task CreateSubmission_SubmissionIdComesFromTheFirstSubmitter() {
		const string json = """
			[
				{ "id": 3001, "submission_id": 2001, "email": "signer1@example.com", "status": "sent" },
				{ "id": 3002, "submission_id": 2001, "email": "signer2@example.com", "status": "awaiting" }
			]
			""";

		var docuSeal = TestClients.CreateWithJson(json, out var handler);

		var response = await docuSeal.CreateSubmissionAsync(CreateSubmissionRequest());

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Post);
		response.SubmissionId.Should().Be(new SubmissionId(2001));
		response.SubmissionId.Should().NotBe(new SubmissionId(3001), "`id` is the submitter, not the submission");
		response.Submitters.Should().HaveCount(2);
	}

	[Fact]
	public async Task CreateSubmission_ErrorObject_ReturnsTheError_AndNoSubmitters() {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Template not found" }""", out _, System.Net.HttpStatusCode.NotFound);

		var response = await docuSeal.CreateSubmissionAsync(CreateSubmissionRequest());

		response.Success.Should().BeFalse();
		response.Errors.Should().Equal("Template not found");
		response.SubmissionId.Should().BeNull();
		response.Submitters.Should().BeEmpty();
	}

	//	============================================================================
	//	UpdateSubmitter
	//	============================================================================

	[Fact]
	public async Task UpdateSubmitter_BindsSpecExample_WithEmbedSrc() {
		var docuSeal = TestClients.CreateWithJson(Spec("submitter-update"), out var handler);

		var response = await docuSeal.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Id = new SubmitterId(1),
			Name = "John Doe"
		});

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Put);
		response.Success.Should().BeTrue();

		var submitter = response.Submitter!;

		submitter.Id.Should().Be(new SubmitterId(1));
		submitter.SubmissionId.Should().Be(new SubmissionId(12));
		submitter.Uuid.Should().Be(Guid.Parse("0954d146-db8c-4772-aafe-2effc7c0e0c0"));
		submitter.Email.Should().Be("submitter@example.com");
		submitter.Slug.Should().Be("dsEeWrhRD8yDXT");
		submitter.SentAtUtc.Should().Be(Utc(2023, 12, 14, 15, 45, 49, 11));
		submitter.OpenedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 48, 23, 11));
		submitter.CompletedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 49, 21, 701));
		submitter.DeclinedAtUtc.Should().BeNull();
		submitter.Name.Should().Be("John Doe");
		submitter.Phone.Should().Be("+1234567890");
		submitter.Status.Should().Be(SubmitterStatus.Completed);
		submitter.ExternalId.Should().BeNull();
		submitter.Values.Should().ContainSingle().Which.Field.Should().Be("Full Name");
		submitter.Documents.Should().BeEmpty();
		submitter.Role.Should().Be("First Party");
		submitter.EmbedSrc.Should().Be(new Uri("https://docuseal.com/s/pAMimKcyrLjqVt"));
	}

	[Fact]
	public async Task UpdateSubmitter_ParsesUpdatedFixture_WithEmbedSrc() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Id = ClientOperations.SubmitterId,
			Name = "Signer One"
		});

		response.Success.Should().BeTrue();
		response.Submitter!.Id.Should().Be(ClientOperations.SubmitterId);
		response.Submitter.EmbedSrc.Should().Be(new Uri("https://docuseal.example.com/s/dsEeWrhRD8yDXT"));
	}

	[Fact]
	public async Task GetSubmitter_HasNoEmbedSrc() {
		var docuSeal = TestClients.CreateWithJson(Spec("submitter"), out _);

		var response = await docuSeal.GetSubmitterAsync(new SubmitterId(7));

		response.Success.Should().BeTrue();
		response.Submitter!.EmbedSrc.Should().BeNull("only the create and update responses carry embed_src");
	}

	//	============================================================================
	//	UpdateTemplate
	//	============================================================================

	[Fact]
	public async Task UpdateTemplate_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("template-update"), out var handler);

		var response = await docuSeal.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = new TemplateId(1),
			Name = "Renamed Template"
		});

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Put);
		response.Success.Should().BeTrue();
		response.Id.Should().Be(new TemplateId(1));
		response.UpdatedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
	}

	[Fact]
	public async Task UpdateTemplate_ParsesUpdatedFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = ClientOperations.TemplateId,
			Name = "Renamed Template"
		});

		response.Success.Should().BeTrue();
		response.Id.Should().Be(ClientOperations.TemplateId);
		response.UpdatedAtUtc.Should().Be(Utc(2024, 8, 5, 17, 0, 0, 0));
	}

	//	============================================================================
	//	CreateTemplateFromHtml
	//	============================================================================

	[Fact]
	public async Task CreateTemplateFromHtml_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("template-html"), out var handler);

		var response = await docuSeal.CreateTemplateFromHtmlAsync(new CreateTemplateFromHtml.Request {
			Html = "<p>Test</p>",
			Name = "Demo Template"
		});

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Post);
		response.Success.Should().BeTrue();

		var template = response.Template!;

		template.Id.Should().Be(new TemplateId(3));
		template.Slug.Should().Be("ZQpF222rFBv71q");
		template.Name.Should().Be("Demo Template");
		template.Schemas.Should().ContainSingle().Which.Name.Should().Be("Demo Template");
		template.Fields.Should().ContainSingle().Which.Name.Should().Be("Name");
		template.Submitters.Should().ContainSingle().Which.Name.Should().Be("Submitter");
		template.Source.Should().Be(TemplateSource.Api);
		template.Folder.Should().Be("Default");
		template.ExternalId.Should().Be("f0b4714f-e44b-4993-905b-68b4451eef8c");
		template.HasSharedLink.Should().BeTrue();
		template.Documents.Should().ContainSingle().Which.Url.Should().Be(new Uri("https://docuseal.com/file/hash/Test%20Template.pdf"));
	}

	[Fact]
	public async Task CreateTemplateFromHtml_ReturnsApiError() {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Invalid HTML" }""", out _, System.Net.HttpStatusCode.UnprocessableEntity);

		var response = await docuSeal.CreateTemplateFromHtmlAsync(new CreateTemplateFromHtml.Request {
			Html = "<p>Test</p>"
		});

		response.Success.Should().BeFalse();
		response.Errors.Should().Equal("Invalid HTML");
		response.Template.Should().BeNull();
	}

	//	============================================================================
	//	UpdateTemplateDocuments
	//	============================================================================

	[Fact]
	public async Task UpdateTemplateDocuments_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("template-documents"), out var handler);

		var response = await docuSeal.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					FileBase64 = "JVBERi0xLjQK",
					Name = "Second Document"
				}
			],
			Id = new TemplateId(3)
		});

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Put);
		response.Success.Should().BeTrue();

		var template = response.Template!;

		template.Id.Should().Be(new TemplateId(3));
		template.Slug.Should().Be("ZQpF222rFBv71q");
		template.Name.Should().Be("Demo Template");
		template.Schemas.Should().ContainSingle().Which.Name.Should().Be("Demo Template");
		template.Fields.Should().ContainSingle().Which.Name.Should().Be("Name");
		template.Submitters.Should().ContainSingle().Which.Name.Should().Be("Submitter");
		template.AuthorId.Should().Be(new UserId(1));
		template.ArchivedAtUtc.Should().BeNull();
		template.Source.Should().Be(TemplateSource.Api);
		template.Folder.Should().Be("Default");
		template.ExternalId.Should().Be("f0b4714f-e44b-4993-905b-68b4451eef8c");
		template.Author.Email.Should().Be("john.doe@example.com");

		var document = template.Documents.Should().ContainSingle().Subject;

		document.Id.Should().Be(new DocumentId(3));
		document.Url.Should().Be(new Uri("https://docuseal.com/file/hash/Test%20Template.pdf"));
	}

	[Fact]
	public async Task UpdateTemplateDocuments_ParsesTemplateFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await ClientOperations.InvokeAsync(docuSeal, nameof(IDocuSealClient.UpdateTemplateDocumentsAsync));

		response.Success.Should().BeTrue();
		response.Payload.Should().BeOfType<Template>().Which.Id.Should().Be(ClientOperations.TemplateId);
	}

	//	============================================================================
	//	ArchiveTemplate
	//	============================================================================

	[Fact]
	public async Task ArchiveTemplate_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("template-archive"), out var handler);

		var response = await docuSeal.ArchiveTemplateAsync(new TemplateId(1));

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Delete);
		response.Success.Should().BeTrue();
		response.Id.Should().Be(new TemplateId(1));
		response.ArchivedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
	}

	[Fact]
	public async Task ArchiveTemplate_ParsesArchivedFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.ArchiveTemplateAsync(ClientOperations.TemplateId);

		response.Success.Should().BeTrue();
		response.Id.Should().Be(ClientOperations.TemplateId);
		response.ArchivedAtUtc.Should().Be(Utc(2024, 8, 5, 17, 0, 0, 0));
	}

	//	============================================================================
	//	ArchiveSubmission
	//	============================================================================

	[Fact]
	public async Task ArchiveSubmission_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("submission-archive"), out var handler);

		var response = await docuSeal.ArchiveSubmissionAsync(new SubmissionId(1));

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Delete);
		response.Success.Should().BeTrue();
		response.Id.Should().Be(new SubmissionId(1));
		response.ArchivedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 50, 21, 799));
	}

	[Fact]
	public async Task ArchiveSubmission_ParsesArchivedFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.ArchiveSubmissionAsync(ClientOperations.SubmissionId);

		response.Success.Should().BeTrue();
		response.Id.Should().Be(ClientOperations.SubmissionId);
		response.ArchivedAtUtc.Should().Be(Utc(2024, 8, 5, 17, 0, 0, 0));
	}

	//	============================================================================
	//	UpdateSubmission
	//	============================================================================

	[Fact]
	public async Task UpdateSubmission_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("submission-update"), out var handler);

		var response = await docuSeal.UpdateSubmissionAsync(new UpdateSubmission.Request {
			Id = new SubmissionId(1),
			Name = "New Submission Name"
		});

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Put);
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
		submission.CompletedAtUtc.Should().Be(Utc(2023, 12, 14, 15, 49, 21, 701));
		submission.CreatedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 48, 17, 166));
		submission.UpdatedAtUtc.Should().Be(Utc(2023, 12, 10, 15, 49, 21, 895));
		submission.ArchivedAtUtc.Should().BeNull();
		submission.CreatedBy!.Email.Should().Be("bob.smith@example.com");
		submission.Template!.Id.Should().Be(new TemplateId(1));
		submission.Events.Should().BeEmpty("the update result carries no submission_events");

		var document = submission.Documents.Should().ContainSingle().Subject;

		document.Name.Should().Be("example");
		document.Url.Should().Be(new Uri("https://docuseal.com/file/hash/example.pdf"));

		var submitter = submission.Submitters.Should().ContainSingle().Subject;

		submitter.Id.Should().Be(new SubmitterId(1));
		submitter.SubmissionId.Should().Be(new SubmissionId(1));
		submitter.Status.Should().Be(SubmitterStatus.Completed);
		submitter.Role.Should().Be("First Party");
	}

	[Fact]
	public async Task UpdateSubmission_ParsesUpdatedFixture() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.UpdateSubmissionAsync(new UpdateSubmission.Request {
			Id = ClientOperations.SubmissionId,
			Name = "Renamed Submission"
		});

		response.Success.Should().BeTrue();
		response.Submission!.Id.Should().Be(ClientOperations.SubmissionId);
		response.Submission.Name.Should().Be("Renamed Submission");
		response.Submission.Documents.Should().ContainSingle();
	}

	[Fact]
	public async Task UpdateSubmission_ReturnsApiError() {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Submission not found" }""", out _, System.Net.HttpStatusCode.NotFound);

		var response = await docuSeal.UpdateSubmissionAsync(new UpdateSubmission.Request {
			Id = ClientOperations.SubmissionId,
			IsArchived = false
		});

		response.Success.Should().BeFalse();
		response.Errors.Should().Equal("Submission not found");
		response.Submission.Should().BeNull();
	}

	//	============================================================================
	//	GetSubmissionDocuments
	//	============================================================================

	[Fact]
	public async Task GetSubmissionDocuments_BindsSpecExample() {
		var docuSeal = TestClients.CreateWithJson(Spec("submission-documents"), out var handler);

		var response = await docuSeal.GetSubmissionDocumentsAsync(new SubmissionId(1));

		handler.Requests.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Get);
		response.Success.Should().BeTrue();
		response.Id.Should().Be(new SubmissionId(1));

		var document = response.Documents.Should().ContainSingle().Subject;

		document.Name.Should().Be("example");
		document.Url.Should().Be(new Uri("https://docuseal.com/file/hash/example.pdf"));
	}

	[Fact]
	public async Task GetSubmissionDocuments_ParsesDocumentsFixture_WithAndWithoutMerge() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await docuSeal.GetSubmissionDocumentsAsync(ClientOperations.SubmissionId);
		var merged = await docuSeal.GetSubmissionDocumentsAsync(new GetSubmissionDocuments.Request {
			Id = ClientOperations.SubmissionId,
			MustMerge = true
		});

		foreach (var result in new[] { response, merged }) {
			result.Success.Should().BeTrue();
			result.Id.Should().Be(ClientOperations.SubmissionId);
			result.Documents.Should().ContainSingle().Which.Url.Should().Be(new Uri("https://docuseal.example.com/file/test-document-signed.pdf"));
		}
	}

	[Fact]
	public async Task GetSubmissionDocuments_ReturnsApiError() {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Submission not found" }""", out _, System.Net.HttpStatusCode.NotFound);

		var response = await docuSeal.GetSubmissionDocumentsAsync(ClientOperations.SubmissionId);

		response.Success.Should().BeFalse();
		response.Errors.Should().Equal("Submission not found");
		response.Id.Should().BeNull();
		response.Documents.Should().BeEmpty();
	}
}
