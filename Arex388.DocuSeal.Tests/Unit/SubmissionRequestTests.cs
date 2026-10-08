using FluentAssertions;
using System.Text.Json.Nodes;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// The submission and submitter request members, query-string encoding, and
/// validation rules #13 added, checked against the OpenAPI spec's example values.
/// </summary>
public sealed class SubmissionRequestTests {
	private static async Task<CapturedRequest> CaptureAsync(
		Func<IDocuSealClient, Task> act,
		string json = "{}") {
		var docuSeal = TestClients.CreateWithJson(json, out var handler);

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

	//	On a UTC host a Local value already reads as UTC, so the test could not tell a dropped conversion apart.
	private static void SkipWhenLocalIsUtc(
		DateTimeOffset instant) => Assert.SkipWhen(TimeZoneInfo.Local.GetUtcOffset(instant) == TimeSpan.Zero, "The local time zone is UTC at this instant, so a Local value cannot reveal a missing UTC conversion.");

	private static Task<CapturedRequest> CaptureCreateSubmissionAsync(
		CreateSubmission.Request request) => CaptureAsync(c => c.CreateSubmissionAsync(request), "[]");

	private static CreateSubmission.Request CreateSubmissionRequest(
		CreateSubmission.RequestSubmitter submitter,
		CreateSubmission.RequestMessage? message = null,
		DateTime? expireAtUtc = null) => new() {
			ExpireAtUtc = expireAtUtc,
			Message = message,
			Submitters = [
				submitter
			],
			TemplateId = ClientOperations.TemplateId
		};

	private static CreateSubmission.RequestSubmitter Submitter(
		CreateSubmission.RequestSubmitterField field) => new() {
			Email = "john.doe@example.com",
			Fields = [
				field
			]
		};

	private static UpdateSubmitter.Request UpdateSubmitterRequest(
		UpdateSubmitter.RequestField? field = null,
		UpdateSubmitter.RequestMessage? message = null) => new() {
			Fields = field is null
				? null
				: [
					field
				],
			Id = ClientOperations.SubmitterId,
			Message = message
		};

	//	============================================================================
	//	ListSubmissions
	//	============================================================================

	[Fact]
	public async Task ListSubmissions_WithPendingStatus_SendsStatusPending() {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			Status = SubmissionStatus.Pending
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions?limit=10&status=pending");
	}

	[Theory]
	[InlineData(SubmissionStatus.Completed, "completed")]
	[InlineData(SubmissionStatus.Declined, "declined")]
	[InlineData(SubmissionStatus.Expired, "expired")]
	[InlineData(SubmissionStatus.Pending, "pending")]
	public async Task ListSubmissions_SendsStatusToken(
		SubmissionStatus status,
		string expected) {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			Status = status
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions?limit=10&status={expected}");
	}

	[Fact]
	public async Task ListSubmissions_BuildsEndpoint_WithEveryNewParameter() {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			After = new SubmissionId(10),
			Before = new SubmissionId(20),
			IsArchived = true,
			Slug = "NtLDQM7eJX2ZMd",
			Status = SubmissionStatus.Completed
		}));

		//	archived=true must be lowercase; the API silently ignores archived=True (#1).
		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions?limit=10&status=completed&slug=NtLDQM7eJX2ZMd&archived=true&after=10&before=20");
	}

	[Fact]
	public async Task ListSubmissions_SendsArchivedFalse_WhenFalse() {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			IsArchived = false
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions?limit=10&archived=false");
	}

	[Fact]
	public async Task ListSubmissions_EncodesEveryStringValue() {
		var request = await CaptureAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			Folder = "Sales & Legal",
			Search = "a b&c",
			Slug = "a/b?c"
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions?limit=10&template_folder=Sales%20%26%20Legal&q=a%20b%26c&slug=a%2Fb%3Fc");
	}

	[Theory]
	[InlineData(SubmissionStatus.Unknown)]
	[InlineData((SubmissionStatus)42)]
	public Task ListSubmissions_StatusWithoutToken_ReturnsInvalid(
		SubmissionStatus status) => ShouldBeInvalidAsync(c => c.ListSubmissionsAsync(new ListSubmissions.Request {
			Status = status
		}), "'Status' must be a known submission status.");

	//	============================================================================
	//	ListSubmitters
	//	============================================================================

	[Fact]
	public async Task ListSubmitters_BuildsEndpoint_WithEveryNewParameter() {
		var request = await CaptureAsync(c => c.ListSubmittersAsync(new ListSubmitters.Request {
			After = new SubmitterId(10),
			Before = new SubmitterId(20),
			CompletedAfterUtc = new DateTime(2024, 3, 5, 9, 32, 20, DateTimeKind.Utc),
			CompletedBeforeUtc = new DateTime(2024, 3, 6, 19, 32, 20, DateTimeKind.Utc),
			ExternalId = "unique-key",
			Slug = "zAyL9fH36Havvm"
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submitters?limit=10&slug=zAyL9fH36Havvm&completed_after=2024-03-05T09%3A32%3A20Z&completed_before=2024-03-06T19%3A32%3A20Z&external_id=unique-key&after=10&before=20");
	}

	[Fact]
	public async Task ListSubmitters_EncodesEveryStringValue() {
		var request = await CaptureAsync(c => c.ListSubmittersAsync(new ListSubmitters.Request {
			ExternalId = "id=1&2",
			Search = "a b&c",
			Slug = "a/b?c"
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submitters?limit=10&q=a%20b%26c&slug=a%2Fb%3Fc&external_id=id%3D1%262");
	}

	[Fact]
	public async Task ListSubmitters_ConvertsLocalCompletedAfter_ToIso8601Utc() {
		var instant = new DateTimeOffset(2024, 3, 5, 9, 32, 20, 500, TimeSpan.FromHours(-5));

		SkipWhenLocalIsUtc(instant);

		var request = await CaptureAsync(c => c.ListSubmittersAsync(new ListSubmitters.Request {
			CompletedAfterUtc = instant.LocalDateTime
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submitters?limit=10&completed_after=2024-03-05T14%3A32%3A20.5Z");
	}

	[Fact]
	public async Task ListSubmitters_TakesUnspecifiedCompletedBefore_AsUtc() {
		var request = await CaptureAsync(c => c.ListSubmittersAsync(new ListSubmitters.Request {
			CompletedBeforeUtc = new DateTime(2024, 3, 6, 19, 32, 20, DateTimeKind.Unspecified)
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submitters?limit=10&completed_before=2024-03-06T19%3A32%3A20Z");
	}

	//	============================================================================
	//	CreateSubmission bodies
	//	============================================================================

	[Fact]
	public async Task CreateSubmission_WritesSpecBody() {
		var request = await CaptureCreateSubmissionAsync(new CreateSubmission.Request {
			ExpireAtUtc = new DateTime(2024, 9, 1, 12, 0, 0, DateTimeKind.Utc),
			Message = new CreateSubmission.RequestMessage {
				Body = "Hi {{submission.name}}, sign at {{submitter.link}}.",
				Subject = "Please sign"
			},
			MustEmail = true,
			MustSms = false,
			OnCompletedBccEmail = "bcc@example.com",
			OnCompletedUrl = "https://example.com/done",
			Order = SubmitterOrder.Preserved,
			ReplyToEmail = "reply@example.com",
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = "john.doe@example.com",
					ExternalId = "2321",
					Fields = [
						new CreateSubmission.RequestSubmitterField {
							DefaultValue = "Acme",
							Description = "Your **legal** name.",
							IsReadonly = false,
							IsRequired = true,
							Name = "First Name",
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
							Title = "First *name*",
							Validation = new RequestFieldValidation {
								Max = "2024-12-31",
								Message = "Four capital letters.",
								Min = 1,
								Pattern = "[A-Z]{4}",
								Step = 0.01M
							}
						}
					],
					InviteBy = "Agent",
					IsCompleted = false,
					Message = new CreateSubmission.RequestMessage {
						Body = "Sign {{template.name}} for {{account.name}}.",
						Subject = "Your turn"
					},
					Metadata = new Dictionary<string, object?> {
						["customField"] = "value"
					},
					MustEmail = true,
					MustSms = false,
					Name = "John Doe",
					OnCompletedUrl = "https://example.com/john",
					OrderGroup = 0,
					Phone = "+1234567890",
					ReplyToEmail = "agent@example.com",
					RequireEmail2fa = true,
					RequirePhone2fa = false,
					Role = "First Party",
					Roles = [
						"First Party",
						"Second Party"
					],
					Values = new Dictionary<string, object?> {
						["First Name"] = "John",
						["Age"] = 30,
						["Agree"] = true,
						["Colors"] = new List<string> {
							"Red",
							"Blue"
						}
					}
				}
			],
			TemplateId = new TemplateId(1000001),
			Variables = new Dictionary<string, object?> {
				["variable_name"] = "value"
			}
		});

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions");
		ShouldBeJson(request.Body, """
			{
				"template_id": 1000001,
				"send_email": true,
				"send_sms": false,
				"order": "preserved",
				"completed_redirect_url": "https://example.com/done",
				"bcc_completed": "bcc@example.com",
				"reply_to": "reply@example.com",
				"expire_at": "2024-09-01 12:00:00 UTC",
				"variables": { "variable_name": "value" },
				"message": {
					"subject": "Please sign",
					"body": "Hi {{submission.name}}, sign at {{submitter.link}}."
				},
				"submitters": [
					{
						"name": "John Doe",
						"role": "First Party",
						"email": "john.doe@example.com",
						"phone": "+1234567890",
						"values": { "First Name": "John", "Age": 30, "Agree": true, "Colors": [ "Red", "Blue" ] },
						"external_id": "2321",
						"completed": false,
						"metadata": { "customField": "value" },
						"send_email": true,
						"send_sms": false,
						"reply_to": "agent@example.com",
						"completed_redirect_url": "https://example.com/john",
						"order": 0,
						"require_phone_2fa": false,
						"require_email_2fa": true,
						"invite_by": "Agent",
						"message": {
							"subject": "Your turn",
							"body": "Sign {{template.name}} for {{account.name}}."
						},
						"fields": [
							{
								"name": "First Name",
								"default_value": "Acme",
								"readonly": false,
								"required": true,
								"title": "First *name*",
								"description": "Your **legal** name.",
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
						],
						"roles": [ "First Party", "Second Party" ]
					}
				]
			}
			""");
	}

	[Fact]
	public async Task CreateSubmission_NestsFieldValidationPattern() {
		var request = await CaptureCreateSubmissionAsync(CreateSubmissionRequest(Submitter(new CreateSubmission.RequestSubmitterField {
			Name = "Code",
			Validation = new RequestFieldValidation {
				Message = "Four capital letters.",
				Pattern = "[A-Z]{4}"
			}
		})));

		ShouldBeJson(request.Body, """{"submitters":[{"email":"john.doe@example.com","fields":[{"name":"Code","validation":{"pattern":"[A-Z]{4}","message":"Four capital letters."}}]}],"template_id":1001}""");
		request.Body.Should().NotContain("invalid_message").And.NotContain("validation_pattern");
	}

	[Fact]
	public async Task CreateSubmission_WritesNonStringDefaultValue() {
		var request = await CaptureCreateSubmissionAsync(CreateSubmissionRequest(Submitter(new CreateSubmission.RequestSubmitterField {
			DefaultValue = new object[] {
				"A",
				2,
				true
			},
			Name = "Choices"
		})));

		ShouldBeJson(request.Body, """{"submitters":[{"email":"john.doe@example.com","fields":[{"name":"Choices","default_value":["A",2,true]}]}],"template_id":1001}""");
	}

	[Fact]
	public async Task CreateSubmission_ConvertsLocalExpireAt_ToUtc() {
		var instant = new DateTimeOffset(2024, 9, 1, 12, 0, 0, TimeSpan.FromHours(-5));

		SkipWhenLocalIsUtc(instant);

		var request = await CaptureCreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
			Email = "john.doe@example.com"
		}, expireAtUtc: instant.LocalDateTime));

		ShouldBeJson(request.Body, """{"expire_at":"2024-09-01 17:00:00 UTC","submitters":[{"email":"john.doe@example.com"}],"template_id":1001}""");
	}

	[Fact]
	public async Task CreateSubmission_TakesUnspecifiedExpireAt_AsUtc() {
		var request = await CaptureCreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
			Email = "john.doe@example.com"
		}, expireAtUtc: new DateTime(2024, 9, 1, 12, 0, 0, DateTimeKind.Unspecified)));

		ShouldBeJson(request.Body, """{"expire_at":"2024-09-01 12:00:00 UTC","submitters":[{"email":"john.doe@example.com"}],"template_id":1001}""");
	}

	//	============================================================================
	//	CreateSubmission validation
	//	============================================================================

	[Fact]
	public async Task CreateSubmission_PhoneOnlySubmitter_PassesValidation() {
		var request = await CaptureCreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
			MustSms = true,
			Phone = "+1234567890"
		}));

		ShouldBeJson(request.Body, """{"submitters":[{"phone":"+1234567890","send_sms":true}],"template_id":1001}""");
	}

	[Fact]
	public Task CreateSubmission_SubmitterWithoutEmailOrPhone_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
		Name = "John Doe"
	})), "'Email' or 'Phone' must be set.");

	[Fact]
	public Task CreateSubmission_SubmitterWithInvalidEmail_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
		Email = "not-an-email",
		Phone = "+1234567890"
	})), "'Email' is not a valid email address.");

	[Fact]
	public async Task CreateSubmission_MessageWithOnlySubject_PassesValidation() {
		var request = await CaptureCreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
			Email = "john.doe@example.com"
		}, new CreateSubmission.RequestMessage {
			Subject = "Please sign"
		}));

		ShouldBeJson(request.Body, """{"message":{"subject":"Please sign"},"submitters":[{"email":"john.doe@example.com"}],"template_id":1001}""");
	}

	[Fact]
	public Task CreateSubmission_EmptyMessage_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
		Email = "john.doe@example.com"
	}, new CreateSubmission.RequestMessage())), "'Body' or 'Subject' must be set.");

	[Fact]
	public Task CreateSubmission_EmptySubmitterMessage_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
		Email = "john.doe@example.com",
		Message = new CreateSubmission.RequestMessage()
	})), "'Body' or 'Subject' must be set.");

	[Fact]
	public Task CreateSubmission_NegativeOrderGroup_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(new CreateSubmission.RequestSubmitter {
		Email = "john.doe@example.com",
		OrderGroup = -1
	})), "'Order Group' must be greater than or equal to '0'.");

	[Fact]
	public Task CreateSubmission_UnknownFieldPreferenceEnum_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(Submitter(new CreateSubmission.RequestSubmitterField {
		Name = "Amount",
		Preferences = new RequestFieldPreferences {
			Currency = Currency.Unknown
		}
	}))), "'Currency' must not be unknown.");

	[Fact]
	public Task CreateSubmission_NonScalarFieldValidationMin_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionAsync(CreateSubmissionRequest(Submitter(new CreateSubmission.RequestSubmitterField {
		Name = "Amount",
		Validation = new RequestFieldValidation {
			Min = new object()
		}
	}))), "'Min' must be a number or a string.");

	//	============================================================================
	//	UpdateSubmitter
	//	============================================================================

	[Fact]
	public async Task UpdateSubmitter_WritesSpecBody() {
		var request = await CaptureAsync(c => c.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Email = "john.doe@example.com",
			ExternalId = "2321",
			Fields = [
				new UpdateSubmitter.RequestField {
					DefaultValue = "Acme",
					IsReadonly = true,
					IsRequired = true,
					Name = "First Name",
					Preferences = new RequestFieldPreferences {
						FontSize = 12,
						FontType = FieldFontType.Bold
					},
					Validation = new RequestFieldValidation {
						Message = "Four capital letters.",
						Pattern = "[A-Z]{4}"
					}
				}
			],
			Id = ClientOperations.SubmitterId,
			IsCompleted = false,
			Message = new UpdateSubmitter.RequestMessage {
				Body = "Sign {{template.name}} at {{submitter.link}}.",
				Subject = "Please sign"
			},
			Metadata = new Dictionary<string, object?> {
				["customField"] = "value"
			},
			Name = "John Doe",
			OnCompletedUrl = "https://example.com/done",
			Phone = "+1234567890",
			ReplyToEmail = "reply@example.com",
			RequireEmail2fa = false,
			RequirePhone2fa = true,
			ResendEmail = true,
			ResendSms = false,
			Values = new Dictionary<string, object?> {
				["First Name"] = "John"
			}
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submitters/3001");
		ShouldBeJson(request.Body, """
			{
				"name": "John Doe",
				"email": "john.doe@example.com",
				"phone": "+1234567890",
				"values": { "First Name": "John" },
				"external_id": "2321",
				"send_email": true,
				"send_sms": false,
				"reply_to": "reply@example.com",
				"completed": false,
				"metadata": { "customField": "value" },
				"completed_redirect_url": "https://example.com/done",
				"require_phone_2fa": true,
				"require_email_2fa": false,
				"message": {
					"subject": "Please sign",
					"body": "Sign {{template.name}} at {{submitter.link}}."
				},
				"fields": [
					{
						"name": "First Name",
						"default_value": "Acme",
						"readonly": true,
						"required": true,
						"validation": {
							"pattern": "[A-Z]{4}",
							"message": "Four capital letters."
						},
						"preferences": {
							"font_size": 12,
							"font_type": "bold"
						}
					}
				]
			}
			""");
		request.Body.Should().NotContain("invalid_message").And.NotContain("validation_pattern");
	}

	[Fact]
	public async Task UpdateSubmitter_WritesMixedTypeValuesAndDefaultValue() {
		var request = await CaptureAsync(c => c.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Fields = [
				new UpdateSubmitter.RequestField {
					DefaultValue = 42,
					Name = "Amount"
				}
			],
			Id = ClientOperations.SubmitterId,
			Values = new Dictionary<string, object?> {
				["First Name"] = "John",
				["Age"] = 30,
				["Agree"] = true,
				["Colors"] = new List<string> {
					"Red",
					"Blue"
				}
			}
		}));

		ShouldBeJson(request.Body, """{"values":{"First Name":"John","Age":30,"Agree":true,"Colors":["Red","Blue"]},"fields":[{"name":"Amount","default_value":42}]}""");
	}

	[Fact]
	public async Task UpdateSubmitter_MessageWithOnlyBody_PassesValidation() {
		var request = await CaptureAsync(c => c.UpdateSubmitterAsync(UpdateSubmitterRequest(message: new UpdateSubmitter.RequestMessage {
			Body = "Please sign."
		})));

		ShouldBeJson(request.Body, """{"message":{"body":"Please sign."}}""");
	}

	[Fact]
	public Task UpdateSubmitter_EmptyMessage_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.UpdateSubmitterAsync(UpdateSubmitterRequest(message: new UpdateSubmitter.RequestMessage())),
		"'Body' or 'Subject' must be set.");

	[Fact]
	public Task UpdateSubmitter_UnknownFieldPreferenceEnum_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.UpdateSubmitterAsync(UpdateSubmitterRequest(new UpdateSubmitter.RequestField {
		Name = "Amount",
		Preferences = new RequestFieldPreferences {
			Align = FieldAlign.Unknown
		}
	})), "'Align' must not be unknown.");

	[Fact]
	public Task UpdateSubmitter_NonScalarFieldValidationMax_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.UpdateSubmitterAsync(UpdateSubmitterRequest(new UpdateSubmitter.RequestField {
		Name = "Amount",
		Validation = new RequestFieldValidation {
			Max = new object()
		}
	})), "'Max' must be a number or a string.");

	//	============================================================================
	//	UpdateSubmission
	//	============================================================================

	[Fact]
	public async Task UpdateSubmission_WithOnlyIsArchivedFalse_SendsOnlyArchivedFalse() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			Id = ClientOperations.SubmissionId,
			IsArchived = false
		}));

		request.Method.Should().Be(HttpMethod.Put);
		request.Uri.Should().Be(new Uri(TestClients.BaseAddress, "submissions/2001"));
		ShouldBeJson(request.Body, """{"archived":false}""");
	}

	[Fact]
	public async Task UpdateSubmission_WithNothingSet_SendsEmptyObject() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			Id = ClientOperations.SubmissionId
		}));

		ShouldBeJson(request.Body, "{}");
	}

	[Fact]
	public async Task UpdateSubmission_WritesSpecBody() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			ExpireAtUtc = new DateTime(2024, 9, 1, 12, 0, 0, DateTimeKind.Utc),
			Id = ClientOperations.SubmissionId,
			IsArchived = true,
			Name = "New Submission Name"
		}));

		ShouldBeJson(request.Body, """{"name":"New Submission Name","expire_at":"2024-09-01 12:00:00 UTC","archived":true}""");
	}

	[Fact]
	public async Task UpdateSubmission_TakesUnspecifiedExpireAt_AsUtc() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			ExpireAtUtc = new DateTime(2024, 9, 1, 12, 0, 0, DateTimeKind.Unspecified),
			Id = ClientOperations.SubmissionId
		}));

		ShouldBeJson(request.Body, """{"expire_at":"2024-09-01 12:00:00 UTC"}""");
	}

	[Fact]
	public async Task UpdateSubmission_ConvertsLocalExpireAt_ToUtc() {
		var instant = new DateTimeOffset(2024, 9, 1, 12, 0, 0, TimeSpan.FromHours(-5));

		SkipWhenLocalIsUtc(instant);

		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			ExpireAtUtc = instant.LocalDateTime,
			Id = ClientOperations.SubmissionId
		}));

		ShouldBeJson(request.Body, """{"expire_at":"2024-09-01 17:00:00 UTC"}""");
	}

	[Fact]
	public async Task UpdateSubmission_WithClearExpiration_SendsExpireAtNull() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			ClearExpiration = true,
			Id = ClientOperations.SubmissionId
		}));

		ShouldBeJson(request.Body, """{"expire_at":null}""");
	}

	[Fact]
	public async Task UpdateSubmission_WithClearExpiration_KeepsTheOtherMembers() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			ClearExpiration = true,
			Id = ClientOperations.SubmissionId,
			IsArchived = false,
			Name = "New Submission Name"
		}));

		ShouldBeJson(request.Body, """{"name":"New Submission Name","archived":false,"expire_at":null}""");
	}

	[Fact]
	public async Task UpdateSubmission_WithoutClearExpiration_OmitsExpireAt() {
		var request = await CaptureAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
			Id = ClientOperations.SubmissionId,
			Name = "New Submission Name"
		}));

		ShouldBeJson(request.Body, """{"name":"New Submission Name"}""");
	}

	[Fact]
	public Task UpdateSubmission_ExpireAtWithClearExpiration_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
		ClearExpiration = true,
		ExpireAtUtc = new DateTime(2024, 9, 1, 12, 0, 0, DateTimeKind.Utc),
		Id = ClientOperations.SubmissionId
	}), "'ExpireAtUtc' and 'ClearExpiration' cannot both be set.");

	[Fact]
	public Task UpdateSubmission_EmptyId_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.UpdateSubmissionAsync(new UpdateSubmission.Request {
		Id = new SubmissionId(0),
		Name = "New Submission Name"
	}), "'Id' must not be empty.");

	//	============================================================================
	//	GetSubmissionDocuments
	//	============================================================================

	[Fact]
	public async Task GetSubmissionDocuments_WithMustMerge_RequestsMergeTrue() {
		var request = await CaptureAsync(c => c.GetSubmissionDocumentsAsync(new GetSubmissionDocuments.Request {
			Id = ClientOperations.SubmissionId,
			MustMerge = true
		}));

		request.Method.Should().Be(HttpMethod.Get);
		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions/2001/documents?merge=true");
	}

	[Theory]
	[InlineData(null)]
	[InlineData(false)]
	public async Task GetSubmissionDocuments_WithoutMustMerge_OmitsTheQuery(
		bool? mustMerge) {
		var request = await CaptureAsync(c => c.GetSubmissionDocumentsAsync(new GetSubmissionDocuments.Request {
			Id = ClientOperations.SubmissionId,
			MustMerge = mustMerge
		}));

		request.Uri.AbsoluteUri.Should().Be($"{TestClients.BaseAddress}submissions/2001/documents");
	}

	[Fact]
	public Task GetSubmissionDocuments_EmptyId_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.GetSubmissionDocumentsAsync(new SubmissionId(0)), "'Id' must not be empty.");

	//	============================================================================
	//	CreateSubmissionFromEmails
	//	============================================================================

	private static CreateSubmissionFromEmails.Request FromEmailsRequest(
		CreateSubmissionFromEmails.RequestMessage? message = null,
		params string[] emails) => new() {
			Emails = emails.Length == 0
				? [
					"a@x.com",
					"b@x.com"
				]
				: emails,
			Message = message,
			TemplateId = new TemplateId(1)
		};

	[Fact]
	public async Task CreateSubmissionFromEmails_WithRequiredMembersOnly_SendsTemplateIdAndJoinedEmails() {
		var request = await CaptureAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest()), "[]");

		request.Method.Should().Be(HttpMethod.Post);
		request.Uri.Should().Be(new Uri(TestClients.BaseAddress, "submissions/emails"));
		ShouldBeJson(request.Body, """{"template_id":1,"emails":"a@x.com,b@x.com"}""");
	}

	[Fact]
	public async Task CreateSubmissionFromEmails_WithOneEmail_SendsItWithoutADelimiter() {
		var request = await CaptureAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest(null, "a@x.com")), "[]");

		ShouldBeJson(request.Body, """{"template_id":1,"emails":"a@x.com"}""");
	}

	[Fact]
	public async Task CreateSubmissionFromEmails_WritesExplicitValues() {
		var request = await CaptureAsync(c => c.CreateSubmissionFromEmailsAsync(new CreateSubmissionFromEmails.Request {
			Emails = [
				"a@x.com",
				"b@x.com"
			],
			Message = new CreateSubmissionFromEmails.RequestMessage {
				Body = "Please sign {{submitter.link}}",
				Subject = "Sign here"
			},
			SendEmail = false,
			TemplateId = new TemplateId(1)
		}), "[]");

		ShouldBeJson(request.Body, """{"template_id":1,"emails":"a@x.com,b@x.com","send_email":false,"message":{"subject":"Sign here","body":"Please sign {{submitter.link}}"}}""");
	}

	[Fact]
	public async Task CreateSubmissionFromEmails_WithOnlyAMessageSubject_OmitsTheBody() {
		var request = await CaptureAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest(new CreateSubmissionFromEmails.RequestMessage {
			Subject = "Sign here"
		})), "[]");

		ShouldBeJson(request.Body, """{"template_id":1,"emails":"a@x.com,b@x.com","message":{"subject":"Sign here"}}""");
	}

	[Fact]
	public Task CreateSubmissionFromEmails_EmptyTemplateId_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionFromEmailsAsync(new CreateSubmissionFromEmails.Request {
		Emails = [
			"a@x.com"
		],
		TemplateId = new TemplateId(0)
	}), "'Template Id' must not be empty.");

	[Fact]
	public Task CreateSubmissionFromEmails_NoEmails_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionFromEmailsAsync(new CreateSubmissionFromEmails.Request {
		Emails = [],
		TemplateId = new TemplateId(1)
	}), "'Emails' must not be empty.");

	[Fact]
	public Task CreateSubmissionFromEmails_MalformedEmail_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest(null, "a@x.com", "not-an-email")), "'Emails' is not a valid email address.");

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public Task CreateSubmissionFromEmails_BlankEmail_ReturnsInvalid(
		string email) => ShouldBeInvalidAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest(null, "a@x.com", email)), "'Emails' must not be empty.");

	[Fact]
	public Task CreateSubmissionFromEmails_EmailWithAComma_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest(null, "a@x.com,b@x.com")), "'Emails' must not contain a comma within an address.");

	[Fact]
	public Task CreateSubmissionFromEmails_EmptyMessage_ReturnsInvalid() => ShouldBeInvalidAsync(c => c.CreateSubmissionFromEmailsAsync(FromEmailsRequest(new CreateSubmissionFromEmails.RequestMessage())), "'Body' or 'Subject' must be set.");
}