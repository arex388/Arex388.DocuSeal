using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace Arex388.DocuSeal.Tests;

public sealed class SubmissionsTest {
	private readonly ITestOutputHelper _console;
	private readonly IDocuSealClient _docuSeal;
	private readonly IHttpClientFactory _httpClientFactory;

	public SubmissionsTest(
		ITestOutputHelper console) {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken1
		}).BuildServiceProvider();

		_console = console;
		_docuSeal = services.GetRequiredService<IDocuSealClient>();
		_httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
	}

	private static async Task<CreateSubmissionFromPdf.RequestDocument> PdfDocumentAsync() => new() {
		Fields = [
			new CreateSubmissionFromPdf.RequestDocumentField {
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
		FileBase64 = Convert.ToBase64String(await File.ReadAllBytesAsync(Utilities.DocuSealFile.FullName)),
		Name = Utilities.DocuSealFile.Name
	};

	[LiveFact]
	public async Task Archive_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);
		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);

		_console.WriteLineWithHeader(nameof(template), template);
		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Act
		//	========================================================================

		var archived = await _docuSeal.ArchiveSubmissionAsync(created.SubmissionId!.Value);

		_console.WriteLineWithHeader(nameof(archived), archived);

		//	========================================================================
		//	Assert
		//	========================================================================

		archived.Errors.Should().BeEmpty();
		archived.Success.Should().BeTrue();

		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}

	[LiveFact]
	public async Task Create_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);

		_console.WriteLineWithHeader(nameof(template), template);

		//	========================================================================
		//	Act
		//	========================================================================

		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);

		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Assert
		//	========================================================================

		created.Errors.Should().BeEmpty();
		created.Success.Should().BeTrue();

		await _docuSeal.ArchiveSubmissionAsync(created.SubmissionId!.Value);
		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}

	[LiveFact]
	public async Task CreateFromPdf_ExpireAt_BindsOnCreateAndGet() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	The API keeps whole seconds, so the expected value carries none below that.
		var now = DateTime.UtcNow.AddDays(30);
		var expireAtUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);

		//	========================================================================
		//	Act
		//	========================================================================

		var created = await _docuSeal.CreateSubmissionFromPdfAsync(new CreateSubmissionFromPdf.Request {
			Documents = [
				await PdfDocumentAsync()
			],
			ExpireAtUtc = expireAtUtc,
			MustEmail = false,
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = Config.Email1,
					Role = "First Party"
				}
			]
		});

		_console.WriteLineWithHeader(nameof(created), created);

		try {
			var gotten = created.Submission is null
				? null
				: await _docuSeal.GetSubmissionAsync(created.Submission.Id);

			_console.WriteLineWithHeader(nameof(gotten), gotten);

			//	====================================================================
			//	Assert
			//	====================================================================

			created.Errors.Should().BeEmpty();
			created.Submission!.ExpireAtUtc.Should().Be(expireAtUtc);
			created.Submission.ExpireAtUtc!.Value.Kind.Should().Be(DateTimeKind.Utc);
			created.Submission.Schemas.Should().ContainSingle();
			created.Submission.Fields.Should().ContainSingle().Which.Type.Should().Be(FieldType.Signature);
			gotten!.Errors.Should().BeEmpty();
			gotten.Submission!.ExpireAtUtc.Should().Be(expireAtUtc);
		} finally {
			if (created.Submission is not null) {
				await _docuSeal.ArchiveSubmissionAsync(created.Submission.Id);
			}
		}
	}

	[LiveFact]
	public async Task Create_SubmitterWithOnlyName_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);

		_console.WriteLineWithHeader(nameof(template), template);

		CreateSubmission.Response? created = null;

		try {
			//	====================================================================
			//	Act
			//	====================================================================

			created = await _docuSeal.CreateSubmissionAsync(new CreateSubmission.Request {
				MustEmail = false,
				Submitters = [
					new CreateSubmission.RequestSubmitter {
						Name = "Live Test Signer",
						Role = "Signer #1"
					}
				],
				TemplateId = template.Template!.Id
			});

			_console.WriteLineWithHeader(nameof(created), created);

			//	====================================================================
			//	Assert
			//	====================================================================

			created.Errors.Should().BeEmpty();
			created.Success.Should().BeTrue();

			var submitter = created.Submitters.Should().ContainSingle().Subject;

			submitter.Name.Should().Be("Live Test Signer");
			submitter.Email.Should().BeNull();
			submitter.Phone.Should().BeNull();
		} finally {
			if (created?.SubmissionId is { } submissionId) {
				await _docuSeal.ArchiveSubmissionAsync(submissionId);
			}

			if (template.Template is not null) {
				await _docuSeal.ArchiveTemplateAsync(template.Template.Id);
			}
		}
	}

	[LiveFact]
	public async Task CreateFromPdf_SubmitterWithoutContact_Fails() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	The library's validator rejects this submitter, so the request goes
		//	straight to the API to prove the API rejects it too.
		var httpClient = _httpClientFactory.CreateClient(nameof(IDocuSealClient));
		var document = await PdfDocumentAsync();
		var body = new JsonObject {
			["documents"] = new JsonArray(new JsonObject {
				["fields"] = new JsonArray(new JsonObject {
					["areas"] = new JsonArray(new JsonObject {
						["h"] = .06,
						["page"] = 1,
						["w"] = .335,
						["x"] = .42,
						["y"] = .15
					}),
					["name"] = "Signature",
					["role"] = "First Party",
					["type"] = "signature"
				}),
				["file"] = document.FileBase64,
				["name"] = document.Name
			}),
			["send_email"] = false,
			["submitters"] = new JsonArray(new JsonObject {
				["role"] = "First Party"
			})
		};

		//	========================================================================
		//	Act
		//	========================================================================

		using var response = await httpClient.PostAsync("submissions/pdf", new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"));

		var content = await response.Content.ReadAsStringAsync();

		_console.WriteLineWithHeader(nameof(response), new {
			StatusCode = (int)response.StatusCode,
			Content = content
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		if (response.IsSuccessStatusCode
			&& JsonNode.Parse(content)?["id"]?.GetValue<int>() is int id) {
			await _docuSeal.ArchiveSubmissionAsync(new SubmissionId(id));
		}

		response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
		JsonNode.Parse(content)!["error"]!.GetValue<string>().Should().Be("email or phone or name is required in `submitters[0]`.");
	}

	[LiveFact]
	public async Task CreateFromPdf_SubmitterWithOnlyName_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var request = new CreateSubmissionFromPdf.Request {
			Documents = [
				await PdfDocumentAsync()
			],
			MustEmail = false,
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Name = "Live Test Signer",
					Role = "First Party"
				}
			]
		};

		//	========================================================================
		//	Act
		//	========================================================================

		var created = await _docuSeal.CreateSubmissionFromPdfAsync(request);

		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Assert
		//	========================================================================

		if (created.Submission is not null) {
			await _docuSeal.ArchiveSubmissionAsync(created.Submission.Id);
		}

		created.Errors.Should().BeEmpty();
		created.Success.Should().BeTrue();

		var submitter = created.Submission!.Submitters.Should().ContainSingle().Subject;

		submitter.Name.Should().Be("Live Test Signer");
		submitter.Email.Should().BeNull();
		submitter.Phone.Should().BeNull();
	}

	[LiveFact]
	public async Task Get_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);
		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);

		_console.WriteLineWithHeader(nameof(template), template);
		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Act
		//	========================================================================

		var gotten = await _docuSeal.GetSubmissionAsync(created.SubmissionId!.Value);

		_console.WriteLineWithHeader(nameof(gotten), gotten);

		//	========================================================================
		//	Assert
		//	========================================================================

		gotten.Errors.Should().BeEmpty();
		gotten.Success.Should().BeTrue();
		gotten.Submission.Should().NotBeNull();

		await _docuSeal.ArchiveSubmissionAsync(created.SubmissionId!.Value);
		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}

	[LiveTheory]
	[InlineData(10, true, 0)]
	[InlineData(100, true, 0)]
	[InlineData(101, false, 1)]
	public async Task List_Succeeds(
		int take,
		bool success,
		int errorsCount) {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	========================================================================
		//	Act
		//	========================================================================

		var listed = await _docuSeal.ListSubmissionsAsync(new ListSubmissions.Request {
			Take = take
		});

		_console.WriteLineWithHeader(nameof(listed), listed);

		//	========================================================================
		//	Assert
		//	========================================================================

		listed.Errors.Count.Should().Be(errorsCount);
		listed.Success.Should().Be(success);
		listed.Submissions.Count.Should().Be(listed.Pagination.Count);
	}
}