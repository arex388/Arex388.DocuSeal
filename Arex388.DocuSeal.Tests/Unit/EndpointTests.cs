using FluentAssertions;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

public sealed class EndpointTests {
	private static async Task<CapturedRequest> CaptureAsync(
		Func<IDocuSealClient, Task> act,
		string json = "{}") {
		var docuSeal = TestClients.CreateWithJson(json, out var handler);

		await act(docuSeal);

		return handler.Requests.Should().ContainSingle().Subject;
	}

	private static void ShouldBe(
		CapturedRequest request,
		HttpMethod method,
		string relativeUri) {
		request.Method.Should().Be(method);
		request.Uri.Should().Be(new Uri(TestClients.BaseAddress, relativeUri));
	}

	//	============================================================================
	//	Path-only endpoints
	//	============================================================================

	[Theory]
	[InlineData(nameof(IDocuSealClient.ArchiveSubmissionAsync), "DELETE", "submissions/2001")]
	[InlineData(nameof(IDocuSealClient.ArchiveTemplateAsync), "DELETE", "templates/1001")]
	[InlineData(nameof(IDocuSealClient.CloneTemplateAsync), "POST", "templates/1001/clone")]
	[InlineData(nameof(IDocuSealClient.CreateSubmissionAsync), "POST", "submissions")]
	[InlineData(nameof(IDocuSealClient.CreateTemplateAsync), "POST", "templates/pdf")]
	[InlineData(ClientOperations.CreateTemplateFromFile, "POST", "templates/pdf")]
	[InlineData(nameof(IDocuSealClient.GetSubmissionAsync), "GET", "submissions/2001")]
	[InlineData(nameof(IDocuSealClient.GetSubmitterAsync), "GET", "submitters/3001")]
	[InlineData(nameof(IDocuSealClient.GetTemplateAsync), "GET", "templates/1001")]
	[InlineData(nameof(IDocuSealClient.MergeTemplatesAsync), "POST", "templates/merge")]
	[InlineData(nameof(IDocuSealClient.UpdateSubmitterAsync), "PUT", "submitters/3001")]
	[InlineData(nameof(IDocuSealClient.UpdateTemplateAsync), "PUT", "templates/1001")]
	[InlineData(nameof(IDocuSealClient.UpdateTemplateDocumentsAsync), "PUT", "templates/1001/documents")]
	public async Task Operation_BuildsEndpoint(
		string operation,
		string method,
		string relativeUri) {
		var request = await CaptureAsync(c => ClientOperations.InvokeAsync(c, operation));

		ShouldBe(request, new HttpMethod(method), relativeUri);
	}

	[Fact]
	public async Task CreateTemplate_FromDocxFile_BuildsDocxEndpoint() {
		var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.docx");

		await File.WriteAllBytesAsync(path, [0x50, 0x4B, 0x03, 0x04]);

		try {
			var request = await CaptureAsync(c => c.CreateTemplateAsync(new FileInfo(path)));

			ShouldBe(request, HttpMethod.Post, "templates/docx");
		} finally {
			File.Delete(path);
		}
	}

	[Fact]
	public async Task Requests_CarryTheAuthorizationTokenHeader() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync());

		request.AuthorizationToken.Should().Be(TestClients.AuthorizationToken);
	}

	//	============================================================================
	//	List endpoints
	//	============================================================================

	[Fact]
	public async Task ListSubmissions_BuildsDefaultEndpoint() {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync());

		ShouldBe(request, HttpMethod.Get, "submissions?limit=10");
	}

	[Fact]
	public async Task ListSubmissions_BuildsEndpoint_WithEveryParameter() {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			Folder = "Contracts",
			Search = "signer",
			Take = 25,
			TemplateId = ClientOperations.TemplateId
		}));

		ShouldBe(request, HttpMethod.Get, "submissions?limit=25&template_folder=Contracts&q=signer&template_id=1001");
	}

	[Fact]
	public async Task ListSubmitters_BuildsDefaultEndpoint() {
		var request = await CaptureAsync(c => c.ListSubmittersAsync());

		ShouldBe(request, HttpMethod.Get, "submitters?limit=10");
	}

	[Fact]
	public async Task ListSubmitters_BuildsEndpoint_WithEveryParameter() {
		var request = await CaptureAsync(c => c.ListSubmittersAsync(new ListSubmitters.Request {
			Search = "signer",
			SubmissionId = ClientOperations.SubmissionId,
			Take = 25
		}));

		ShouldBe(request, HttpMethod.Get, "submitters?limit=25&q=signer&submission_id=2001");
	}

	[Fact]
	public async Task ListTemplates_BuildsDefaultEndpoint() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync());

		ShouldBe(request, HttpMethod.Get, "templates?limit=10");
	}

	[Fact]
	public async Task ListTemplates_BuildsEndpoint_WithEveryParameter() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync(new ListTemplates.Request {
			Folder = "Contracts",
			IsArchived = true,
			Search = "test",
			Take = 25
		}));

		//	archived=true must be lowercase; the API silently ignores archived=True (#1).
		ShouldBe(request, HttpMethod.Get, "templates?limit=25&folder=Contracts&archived=true&q=test");
	}

	[Fact]
	public async Task ListTemplates_OmitsArchived_WhenFalse() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync(new ListTemplates.Request {
			IsArchived = false
		}));

		ShouldBe(request, HttpMethod.Get, "templates?limit=10");
	}

	//	============================================================================
	//	Request bodies
	//	============================================================================

	[Fact]
	public async Task CreateSubmission_WritesSnakeCaseBody() {
		var request = await CaptureAsync(c => c.CreateSubmissionAsync(new CreateSubmission.Request {
			Order = SubmitterOrder.Random,
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = "signer1@example.com",
					Role = "First Party"
				}
			],
			TemplateId = ClientOperations.TemplateId
		}), "[]");

		using var body = JsonDocument.Parse(request.Body!);

		body.RootElement.GetProperty("template_id").GetInt32().Should().Be(1001);
		body.RootElement.TryGetProperty("send_email", out _).Should().BeFalse("an unset optional member is omitted so the API default applies");
		body.RootElement.GetProperty("order").GetString().Should().Be("random");
		body.RootElement.GetProperty("submitters")[0].GetProperty("email").GetString().Should().Be("signer1@example.com");
		body.RootElement.TryGetProperty("endpoint", out _).Should().BeFalse("the internal Endpoint must not leak into the body");
	}

	[Fact]
	public async Task CreateTemplate_WritesFieldTypeToken_AndOmitsEndpoint() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(new CreateTemplate.Request {
			Documents = [
				new CreateTemplate.RequestDocument {
					Fields = [
						new CreateTemplate.RequestDocumentField {
							Areas = [
								new CreateTemplate.RequestDocumentFieldArea {
									Height = .06M,
									Page = 1,
									Width = .335M,
									X = .42M,
									Y = .15M
								}
							],
							Name = "Signature",
							Role = "First Party",
							Type = FieldType.Signature
						}
					],
					FileBase64 = "JVBERi0xLjQK",
					Name = "Test Document"
				}
			],
			Endpoint = CreateTemplate.Endpoints.Pdf,
			Name = "Test Template"
		}));

		using var body = JsonDocument.Parse(request.Body!);

		var field = body.RootElement.GetProperty("documents")[0].GetProperty("fields")[0];

		field.GetProperty("type").GetString().Should().Be("signature");
		field.GetProperty("areas")[0].GetProperty("w").GetDecimal().Should().Be(.335M);
		body.RootElement.GetProperty("documents")[0].GetProperty("file").GetString().Should().Be("JVBERi0xLjQK");
		body.RootElement.TryGetProperty("endpoint", out _).Should().BeFalse("Endpoint is [JsonIgnore]");
	}

	[Fact]
	public async Task UpdateSubmitter_OmitsIdFromBody() {
		var request = await CaptureAsync(c => c.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Id = ClientOperations.SubmitterId,
			Name = "Signer One"
		}));

		using var body = JsonDocument.Parse(request.Body!);

		body.RootElement.TryGetProperty("id", out _).Should().BeFalse("Id travels in the path, not the body");
		body.RootElement.GetProperty("name").GetString().Should().Be("Signer One");
	}
}
