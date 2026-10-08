using FluentAssertions;
using System.Text.Json.Nodes;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// The template request members, query-string encoding, and validation rules
/// #12 added, checked against the OpenAPI spec's example values.
/// </summary>
public sealed class TemplateRequestTests {
	private static async Task<CapturedRequest> CaptureAsync(
		Func<IDocuSealClient, Task> act) {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		await act(docuSeal);

		return handler.Requests.Should().ContainSingle().Subject;
	}

	private static void ShouldBeJson(
		string? body,
		string expected) => JsonNode.DeepEquals(JsonNode.Parse(body!), JsonNode.Parse(expected)).Should().BeTrue($"the body was {body}");

	private static async Task ShouldBeInvalidAsync<TResponse>(
		Func<IDocuSealClient, Task<TResponse>> act,
		string error)
		where TResponse : ResponseBase<TResponse>, new() {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var response = await act(docuSeal);

		response.Success.Should().BeFalse();
		response.Errors.Should().Contain(error);
		handler.Requests.Should().BeEmpty("validation failures must not reach the API");
	}

	private static CreateTemplate.Request CreateTemplateRequest(
		string endpoint,
		bool? isDynamic = null,
		bool? mustFlatten = null,
		bool? mustRemoveTags = null,
		CreateTemplate.RequestDocumentField? field = null) => new() {
			Documents = [
				new CreateTemplate.RequestDocument {
					Fields = field is null
						? null
						: [
							field
						],
					FileBase64 = "base64",
					IsDynamic = isDynamic,
					Name = "Test Document"
				}
			],
			Endpoint = endpoint,
			MustFlatten = mustFlatten,
			MustRemoveTags = mustRemoveTags
		};

	//	============================================================================
	//	ListTemplates
	//	============================================================================

	[Theory]
	[InlineData("a&b", "q=a%26b")]
	[InlineData("a b&c", "q=a%20b%26c")]
	public async Task ListTemplates_EncodesSearch(
		string search,
		string expected) {
		var request = await CaptureAsync(c => c.ListTemplatesAsync(new ListTemplates.Request {
			Search = search
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates?limit=10&{expected}");
	}

	[Fact]
	public async Task ListTemplates_BuildsEndpoint_WithEveryNewParameter() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync(new ListTemplates.Request {
			After = new TemplateId(10),
			Before = new TemplateId(20),
			ExternalId = "unique-key",
			IsShared = true,
			Slug = "opaKWh8WWTAcVG"
		}));

		//	shared=true must be lowercase, like archived=true (#1).
		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates?limit=10&slug=opaKWh8WWTAcVG&external_id=unique-key&shared=true&after=10&before=20");
	}

	[Fact]
	public async Task ListTemplates_EncodesEveryStringValue() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync(new ListTemplates.Request {
			ExternalId = "id=1&2",
			Folder = "Sales & Legal",
			Slug = "a/b?c"
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates?limit=10&folder=Sales%20%26%20Legal&slug=a%2Fb%3Fc&external_id=id%3D1%262");
	}

	[Fact]
	public async Task ListTemplates_OmitsShared_WhenFalse() {
		var request = await CaptureAsync(c => c.ListTemplatesAsync(new ListTemplates.Request {
			IsShared = false
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates?limit=10");
	}

	//	============================================================================
	//	CloneTemplate and MergeTemplates
	//	============================================================================

	[Fact]
	public async Task CloneTemplate_WritesSpecBody() {
		var request = await CaptureAsync(c => c.CloneTemplateAsync(new CloneTemplate.Request {
			ExternalId = "unique-key",
			Folder = "Default",
			Id = ClientOperations.TemplateId,
			Name = "Cloned Template"
		}));

		ShouldBeJson(request.Body, """{"name":"Cloned Template","folder_name":"Default","external_id":"unique-key"}""");
	}

	[Fact]
	public async Task MergeTemplates_WritesSpecBody() {
		var request = await CaptureAsync(c => c.MergeTemplatesAsync(new MergeTemplates.Request {
			ExternalId = "unique-key",
			Folder = "Default",
			HasSharedLink = false,
			Ids = [
				new TemplateId(321),
				new TemplateId(432)
			],
			Name = "Merged Template",
			Roles = [
				"Agent",
				"Customer"
			]
		}));

		ShouldBeJson(request.Body, """{"template_ids":[321,432],"name":"Merged Template","folder_name":"Default","external_id":"unique-key","shared_link":false,"roles":["Agent","Customer"]}""");
	}

	//	============================================================================
	//	CreateTemplate bodies
	//	============================================================================

	[Fact]
	public async Task CreateTemplateFromPdf_WritesSpecBody() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(new CreateTemplate.Request {
			Documents = [
				new CreateTemplate.RequestDocument {
					Fields = [
						new CreateTemplate.RequestDocumentField {
							Areas = [
								new CreateTemplate.RequestDocumentFieldArea {
									Height = 0.06M,
									Option = "Option A",
									Page = 1,
									Width = 0.335M,
									X = 0.42M,
									Y = 0.15M
								}
							],
							Description = "Pick **one**.",
							IsRequired = true,
							Name = "Choice",
							Options = [
								"Option A",
								"Option B"
							],
							Preferences = new RequestFieldPreferences {
								Align = FieldAlign.Left,
								Background = "#FF0000",
								Color = "black",
								Currency = Currency.Usd,
								Font = FieldFont.Times,
								FontSize = 12,
								FontType = FieldFontType.BoldItalic,
								Format = "DD/MM/YYYY",
								Mask = true,
								Price = 99.99M,
								Reasons = [
									"Approved"
								],
								VerticalAlign = FieldVerticalAlign.Center
							},
							Role = "First Party",
							Title = "Your *choice*",
							Type = FieldType.Radio,
							Validation = new RequestFieldValidation {
								Max = "2024-12-31",
								Message = "Four capital letters.",
								Min = 1,
								Pattern = "[A-Z]{4}",
								Step = 0.01M
							}
						}
					],
					FileBase64 = "base64",
					Name = "Test Document"
				}
			],
			Endpoint = CreateTemplate.Endpoints.Pdf,
			ExternalId = "unique-key",
			Folder = "Default",
			HasSharedLink = true,
			MustFlatten = false,
			MustRemoveTags = true,
			Name = "Test PDF"
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates/pdf");
		ShouldBeJson(request.Body, """
			{
				"name": "Test PDF",
				"folder_name": "Default",
				"external_id": "unique-key",
				"shared_link": true,
				"documents": [
					{
						"name": "Test Document",
						"file": "base64",
						"fields": [
							{
								"name": "Choice",
								"type": "radio",
								"role": "First Party",
								"required": true,
								"title": "Your *choice*",
								"description": "Pick **one**.",
								"areas": [
									{ "x": 0.42, "y": 0.15, "w": 0.335, "h": 0.06, "page": 1, "option": "Option A" }
								],
								"options": [ "Option A", "Option B" ],
								"validation": {
									"pattern": "[A-Z]{4}",
									"message": "Four capital letters.",
									"min": 1,
									"max": "2024-12-31",
									"step": 0.01
								},
								"preferences": {
									"font_size": 12,
									"font_type": "bold_italic",
									"font": "Times",
									"color": "black",
									"background": "#FF0000",
									"align": "left",
									"valign": "center",
									"format": "DD/MM/YYYY",
									"price": 99.99,
									"currency": "USD",
									"mask": true,
									"reasons": [ "Approved" ]
								}
							}
						]
					}
				],
				"flatten": false,
				"remove_tags": true
			}
			""");
	}

	[Fact]
	public async Task CreateTemplateFromDocx_WritesSpecBody() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(new CreateTemplate.Request {
			Documents = [
				new CreateTemplate.RequestDocument {
					FileBase64 = "base64",
					IsDynamic = true,
					Name = "Test Document"
				}
			],
			Endpoint = CreateTemplate.Endpoints.Docx,
			ExternalId = "unique-key",
			Folder = "Default",
			HasSharedLink = false,
			Name = "Test DOCX"
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates/docx");
		ShouldBeJson(request.Body, """{"name":"Test DOCX","external_id":"unique-key","folder_name":"Default","shared_link":false,"documents":[{"name":"Test Document","file":"base64","dynamic":true}]}""");
	}

	[Fact]
	public async Task CreateTemplate_WithoutNameOrFields_OmitsThem() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf)));

		request.Body.Should().Be("""{"documents":[{"file":"base64","name":"Test Document"}]}""");
	}

	[Fact]
	public async Task CreateTemplate_WritesIntegerMask() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
			Name = "SSN",
			Preferences = new RequestFieldPreferences {
				Mask = 4
			}
		})));

		ShouldBeJson(request.Body, """{"documents":[{"file":"base64","name":"Test Document","fields":[{"name":"SSN","preferences":{"mask":4}}]}]}""");
	}

	//	============================================================================
	//	CreateTemplate validation
	//	============================================================================

	[Fact]
	public async Task CreateTemplate_FieldWithOnlyName_PassesValidation() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
			Name = "Field"
		})));

		request.Body.Should().Be("""{"documents":[{"fields":[{"name":"Field"}],"file":"base64","name":"Test Document"}]}""");
	}

	[Fact]
	public Task CreateTemplate_FieldWithoutName_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
		Name = ""
	})), "'Name' must not be empty.");

	[Theory]
	[InlineData(true, null)]
	[InlineData(null, false)]
	public Task CreateTemplate_PdfOnlyMember_OnDocx_ReturnsInvalid(
		bool? mustFlatten,
		bool? mustRemoveTags) => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Docx, mustFlatten: mustFlatten, mustRemoveTags: mustRemoveTags)),
		mustFlatten.HasValue
			? "'Must Flatten' is only supported when creating a template from a PDF file."
			: "'Must Remove Tags' is only supported when creating a template from a PDF file.");

	[Fact]
	public Task CreateTemplate_DynamicDocument_OnPdf_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, isDynamic: false)),
		"'Is Dynamic' is only supported when creating a template from a DOCX file.");

	[Fact]
	public async Task CreateTemplate_NullDocument_OnPdf_DoesNotThrow() {
		var docuSeal = TestClients.CreateWithJson("{}", out _);

		var act = () => docuSeal.CreateTemplateAsync(new CreateTemplate.Request {
			Documents = [
				null!
			],
			Endpoint = CreateTemplate.Endpoints.Pdf
		});

		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task CreateTemplate_PdfOnlyMembers_OnPdf_PassValidation() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, mustFlatten: true, mustRemoveTags: false)));

		ShouldBeJson(request.Body, """{"documents":[{"file":"base64","name":"Test Document"}],"flatten":true,"remove_tags":false}""");
	}

	[Fact]
	public Task CreateTemplate_StringMask_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
		Name = "SSN",
		Preferences = new RequestFieldPreferences {
			Mask = "yes"
		}
	})), "'Mask' must be a boolean or an integer.");

	[Fact]
	public Task CreateTemplate_UnknownPreferenceEnum_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
		Name = "Amount",
		Preferences = new RequestFieldPreferences {
			Currency = Currency.Unknown
		}
	})), "'Currency' must not be unknown.");

	[Fact]
	public async Task CreateTemplate_AreaAtOrigin_PassesValidation() {
		var request = await CaptureAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
			Areas = [
				new CreateTemplate.RequestDocumentFieldArea {
					Height = 0.06M,
					Page = 1,
					Width = 0.335M,
					X = 0,
					Y = 0
				}
			],
			Name = "Corner"
		})));

		ShouldBeJson(request.Body, """{"documents":[{"file":"base64","name":"Test Document","fields":[{"name":"Corner","areas":[{"h":0.06,"page":1,"w":0.335,"x":0,"y":0}]}]}]}""");
	}

	[Fact]
	public Task CreateTemplate_NegativeAreaCoordinate_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
		Areas = [
			new CreateTemplate.RequestDocumentFieldArea {
				Height = 0.06M,
				Page = 1,
				Width = 0.335M,
				X = -0.1M,
				Y = 0
			}
		],
		Name = "Corner"
	})), "'X' must be greater than or equal to '0'.");

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public Task CreateTemplate_NonFiniteDoubleValidationMax_ReturnsInvalid(
		double max) => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
			Name = "Amount",
			Validation = new RequestFieldValidation {
				Max = max
			}
		})), "'Max' must be a number or a string.");

	[Fact]
	public Task CreateTemplate_NonFiniteFloatValidationMin_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
		Name = "Amount",
		Validation = new RequestFieldValidation {
			Min = float.NaN
		}
	})), "'Min' must be a number or a string.");

	[Fact]
	public Task CreateTemplate_NonScalarValidationMin_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(CreateTemplateRequest(CreateTemplate.Endpoints.Pdf, field: new CreateTemplate.RequestDocumentField {
		Name = "Amount",
		Validation = new RequestFieldValidation {
			Min = new object()
		}
	})), "'Min' must be a number or a string.");

	//	============================================================================
	//	CreateTemplateAsync(FileInfo)
	//	============================================================================

	[Fact]
	public async Task CreateTemplate_FromUnsupportedFile_ReturnsInvalid_WithoutHttpCall() {
		var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");

		await File.WriteAllTextAsync(path, "x");

		try {
			await ShouldBeInvalidAsync(c => c.CreateTemplateAsync(new FileInfo(path)), "'File' must be a .pdf or .docx file.");
		} finally {
			File.Delete(path);
		}
	}

	[Fact]
	public Task CreateTemplate_FromMissingUnsupportedFile_ReturnsInvalid_WithoutHttpCall() => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(new FileInfo($"{Guid.NewGuid():N}.txt")),
		"'File' must be a .pdf or .docx file.");

	[Theory]
	[InlineData(".pdf")]
	[InlineData(".docx")]
	public Task CreateTemplate_FromMissingFile_ReturnsInvalid_WithoutHttpCall(
		string extension) => ShouldBeInvalidAsync(c => c.CreateTemplateAsync(new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}"))),
		"'File' does not exist.");

	[Fact]
	public async Task CreateTemplate_FromUnreadableFile_ReturnsFailed_WithoutHttpCall() {
		Assert.SkipUnless(OperatingSystem.IsWindows(), "An exclusive share lock only blocks reads on Windows.");

		var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

		await File.WriteAllBytesAsync(path, [0x25, 0x50, 0x44, 0x46]);

		try {
			var docuSeal = TestClients.CreateWithJson("{}", out var handler);
			CreateTemplate.Response response;

			using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
				response = await docuSeal.CreateTemplateAsync(new FileInfo(path));
			}

			response.Success.Should().BeFalse();
			response.Errors.Should().Equal("The request has failed.");
			handler.Requests.Should().BeEmpty();
		} finally {
			File.Delete(path);
		}
	}

	[Fact]
	public async Task CreateTemplate_FromUppercasePdfExtension_BuildsPdfEndpoint() {
		var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.PDF");

		await File.WriteAllBytesAsync(path, [0x25, 0x50, 0x44, 0x46]);

		try {
			var request = await CaptureAsync(c => c.CreateTemplateAsync(new FileInfo(path)));

			request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}templates/pdf");
		} finally {
			File.Delete(path);
		}
	}

	//	============================================================================
	//	UpdateTemplateDocuments
	//	============================================================================

	[Fact]
	public Task UpdateTemplateDocuments_Remove_WithoutNameOrPosition_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
		Documents = [
			new UpdateTemplateDocuments.RequestDocument {
				MustRemove = true
			}
		],
		Id = ClientOperations.TemplateId
	}), "'Name' or 'Position' must be set to remove a document.");

	[Fact]
	public async Task UpdateTemplateDocuments_Replace_WithoutNameOrPosition_PassesValidation() {
		var request = await CaptureAsync(c => c.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					FileBase64 = "base64",
					MustReplace = true
				}
			],
			Id = ClientOperations.TemplateId
		}));

		ShouldBeJson(request.Body, """{"documents":[{"file":"base64","replace":true}]}""");
	}

	[Fact]
	public async Task UpdateTemplateDocuments_AllowsRemovalByNameAlone() {
		var request = await CaptureAsync(c => c.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					MustRemove = true,
					Name = "Test Template"
				}
			],
			Id = ClientOperations.TemplateId
		}));

		ShouldBeJson(request.Body, """{"documents":[{"remove":true,"name":"Test Template"}]}""");
	}

	[Fact]
	public async Task UpdateTemplateDocuments_AddsWithoutNameOrPosition() {
		var request = await CaptureAsync(c => c.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					FileBase64 = "base64"
				}
			],
			Id = ClientOperations.TemplateId,
			MustMerge = true
		}));

		ShouldBeJson(request.Body, """{"documents":[{"file":"base64"}],"merge":true}""");
	}
}
