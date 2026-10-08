using FluentAssertions;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Unset optional request members must be omitted from the wire body so the
/// API's own defaults apply (#9).
/// </summary>
public sealed class RequestBodyTests {
	private static readonly JsonSerializerOptions _options = TestClients.JsonOptions;

	private static async Task<string> CaptureBodyAsync(
		Func<IDocuSealClient, Task> act,
		string json = "{}") {
		var docuSeal = TestClients.CreateWithJson(json, out var handler);

		await act(docuSeal);

		return handler.Requests.Should().ContainSingle().Subject.Body!;
	}

	//	============================================================================
	//	UpdateTemplate
	//	============================================================================

	[Fact]
	public async Task UpdateTemplate_WithOnlyName_WritesOnlyName() {
		var body = await CaptureBodyAsync(c => c.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = ClientOperations.TemplateId,
			Name = "x"
		}));

		body.Should().Be("""{"name":"x"}""");
	}

	[Fact]
	public async Task UpdateTemplate_WithNothingSet_WritesEmptyObject() {
		var body = await CaptureBodyAsync(c => c.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = ClientOperations.TemplateId
		}));

		body.Should().Be("{}");
	}

	[Fact]
	public async Task UpdateTemplate_WritesExplicitFalse_ForArchived() {
		var body = await CaptureBodyAsync(c => c.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = ClientOperations.TemplateId,
			IsArchived = false
		}));

		body.Should().Be("""{"archived":false}""");
	}

	[Fact]
	public async Task UpdateTemplate_WritesEmptyRoles_WhenSetExplicitly() {
		var body = await CaptureBodyAsync(c => c.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = ClientOperations.TemplateId,
			Roles = []
		}));

		body.Should().Be("""{"roles":[]}""");
	}

	//	============================================================================
	//	UpdateTemplateDocuments
	//	============================================================================

	[Fact]
	public async Task UpdateTemplateDocuments_WithOnlyFile_OmitsPositionAndName() {
		var body = await CaptureBodyAsync(c => c.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					FileBase64 = "JVBERi0xLjQK"
				}
			],
			Id = ClientOperations.TemplateId
		}));

		body.Should().Be("""{"documents":[{"file":"JVBERi0xLjQK"}]}""");
	}

	[Fact]
	public async Task UpdateTemplateDocuments_AllowsRemovalByPositionAlone() {
		var body = await CaptureBodyAsync(c => c.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					MustRemove = true,
					Position = 0
				}
			],
			Id = ClientOperations.TemplateId
		}));

		body.Should().Be("""{"documents":[{"remove":true,"position":0}]}""");
	}

	[Theory]
	[InlineData(null)]
	[InlineData(false)]
	public async Task UpdateTemplateDocuments_WithoutFileOrHtml_ReturnsInvalid_WithoutHttpCall(
		bool? mustRemove) {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var response = await docuSeal.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					MustRemove = mustRemove,
					Position = 0
				}
			],
			Id = ClientOperations.TemplateId
		});

		response.Success.Should().BeFalse();
		response.Errors.Should().NotBeEmpty();
		handler.Requests.Should().BeEmpty("validation failures must not reach the API");
	}

	//	============================================================================
	//	UpdateSubmitter
	//	============================================================================

	[Fact]
	public async Task UpdateSubmitter_WithOnlyName_WritesOnlyName() {
		var body = await CaptureBodyAsync(c => c.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Id = ClientOperations.SubmitterId,
			Name = "Signer One"
		}));

		body.Should().Be("""{"name":"Signer One"}""");
	}

	[Fact]
	public async Task UpdateSubmitter_WritesExplicitFlags_AndOmitsUnsetFieldMembers() {
		var body = await CaptureBodyAsync(c => c.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Fields = [
				new UpdateSubmitter.RequestField {
					Name = "Signature"
				}
			],
			Id = ClientOperations.SubmitterId,
			IsCompleted = false,
			ResendEmail = true
		}));

		body.Should().Be("""{"fields":[{"name":"Signature"}],"completed":false,"send_email":true}""");
	}

	//	============================================================================
	//	CreateSubmission
	//	============================================================================

	[Fact]
	public async Task CreateSubmission_WithRequiredMembersOnly_OmitsEverythingElse() {
		var body = await CaptureBodyAsync(c => c.CreateSubmissionAsync(new CreateSubmission.Request {
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = "signer1@example.com"
				}
			],
			TemplateId = ClientOperations.TemplateId
		}), "[]");

		body.Should().Be("""{"submitters":[{"email":"signer1@example.com"}],"template_id":1001}""");
	}

	[Fact]
	public async Task CreateSubmission_WritesExplicitValues() {
		var body = await CaptureBodyAsync(c => c.CreateSubmissionAsync(new CreateSubmission.Request {
			MustEmail = false,
			Order = SubmitterOrder.Random,
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = "signer1@example.com",
					MustSms = true
				}
			],
			TemplateId = ClientOperations.TemplateId
		}), "[]");

		body.Should().Be("""{"send_email":false,"order":"random","submitters":[{"email":"signer1@example.com","send_sms":true}],"template_id":1001}""");
	}

	//	============================================================================
	//	CloneTemplate and MergeTemplates
	//	============================================================================

	[Fact]
	public async Task CloneTemplate_OmitsUnsetFolderAndName() {
		var body = await CaptureBodyAsync(c => c.CloneTemplateAsync(new CloneTemplate.Request {
			Id = ClientOperations.TemplateId
		}));

		using var document = JsonDocument.Parse(body);

		document.RootElement.TryGetProperty("folder_name", out _).Should().BeFalse();
		document.RootElement.TryGetProperty("name", out _).Should().BeFalse();
	}

	[Fact]
	public async Task MergeTemplates_OmitsUnsetFolderAndName() {
		var body = await CaptureBodyAsync(c => c.MergeTemplatesAsync(new MergeTemplates.Request {
			Ids = [
				ClientOperations.TemplateId,
				new TemplateId(1002)
			]
		}));

		using var document = JsonDocument.Parse(body);

		document.RootElement.TryGetProperty("folder_name", out _).Should().BeFalse();
		document.RootElement.TryGetProperty("name", out _).Should().BeFalse();
		document.RootElement.GetProperty("template_ids").GetArrayLength().Should().Be(2);
	}

	//	============================================================================
	//	Responses
	//	============================================================================

	[Fact]
	public void Template_WithNullArchivedAt_StillBinds() {
		var template = JsonSerializer.Deserialize<Template>("""{"id":1001,"name":"x","archived_at":null}""", _options)!;

		template.Id.Should().Be(ClientOperations.TemplateId);
		template.Name.Should().Be("x");
		template.ArchivedAtUtc.Should().BeNull();
	}

	[Fact]
	public async Task GetTemplate_WithNullArchivedAt_StillBinds() {
		var docuSeal = TestClients.CreateWithJson("""{"id":1001,"name":"x","archived_at":null}""", out _);

		var response = await docuSeal.GetTemplateAsync(ClientOperations.TemplateId);

		response.Success.Should().BeTrue();
		response.Template.Should().NotBeNull();
		response.Template!.ArchivedAtUtc.Should().BeNull();
	}
}
