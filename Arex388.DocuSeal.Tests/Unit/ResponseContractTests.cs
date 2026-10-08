using FluentAssertions;
using FluentValidation;
using System.Net;
using System.Reflection;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// The no-throw contract: every IDocuSealClient member reports cancellation,
/// validation failures, transport failures, and API error bodies through
/// <c>Errors</c> / <c>Success</c> instead of throwing.
/// </summary>
public sealed class ResponseContractTests {
	private const string _cancelled = "The request was cancelled.";
	private const string _failed = "The request has failed.";

	private static readonly string[] _allOperations = [
		.. ClientOperations.WithPayload,
		.. ClientOperations.Lists
	];

	/// <summary>
	/// An error body with a 4xx status for every operation, plus the lists with a
	/// 200 status (the lists had no error member before, so an error body read as
	/// an empty success).
	/// </summary>
	public static TheoryData<string, HttpStatusCode> ErrorBodyRows {
		get {
			var rows = new TheoryData<string, HttpStatusCode>();

			foreach (var operation in _allOperations) {
				rows.Add(operation, HttpStatusCode.NotFound);
				rows.Add(operation, HttpStatusCode.UnprocessableEntity);
			}

			foreach (var operation in ClientOperations.Lists) {
				rows.Add(operation, HttpStatusCode.OK);
			}

			return rows;
		}
	}

	/// <summary>
	/// A non-2xx status whose body has no usable error member: empty, not JSON,
	/// or a JSON object without <c>error</c>.
	/// </summary>
	public static TheoryData<string, HttpStatusCode, string> UnusableErrorBodyRows {
		get {
			var rows = new TheoryData<string, HttpStatusCode, string>();

			foreach (var operation in _allOperations) {
				rows.Add(operation, HttpStatusCode.NotFound, "");
				rows.Add(operation, HttpStatusCode.BadGateway, "<html><body>Bad Gateway</body></html>");
				rows.Add(operation, HttpStatusCode.InternalServerError, "{}");
			}

			return rows;
		}
	}

	[Theory]
	[MemberData(nameof(ClientOperations.All), MemberType = typeof(ClientOperations))]
	public async Task PreCancelledToken_ReturnsCancelled_WithoutHttpCall(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		using var cts = new CancellationTokenSource();

		await cts.CancelAsync();

		var result = await ClientOperations.InvokeAsync(docuSeal, operation, cts.Token);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be(_cancelled);
		handler.Requests.Should().BeEmpty("a cancelled request must not reach the API");
	}

	[Theory]
	[MemberData(nameof(ClientOperations.AllLists), MemberType = typeof(ClientOperations))]
	public async Task List_TakeAboveMaximum_ReturnsInvalid_WithoutHttpCall(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var result = operation switch {
			nameof(IDocuSealClient.ListSubmissionsAsync) => Shape(await docuSeal.ListSubmissionsAsync(new ListSubmissions.Request {
				Take = 101
			})),
			nameof(IDocuSealClient.ListSubmittersAsync) => Shape(await docuSeal.ListSubmittersAsync(new ListSubmitters.Request {
				Take = 101
			})),
			nameof(IDocuSealClient.ListTemplatesAsync) => Shape(await docuSeal.ListTemplatesAsync(new ListTemplates.Request {
				Take = 101
			})),
			_ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
		};

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be("'Take' must be less than or equal to '100'.");
		handler.Requests.Should().BeEmpty("validation failures must not reach the API");
	}

	[Fact]
	public async Task GetTemplate_EmptyId_ReturnsInvalid_WithoutHttpCall() {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var response = await docuSeal.GetTemplateAsync(new TemplateId(0));

		response.Success.Should().BeFalse();
		response.Errors.Should().ContainSingle().Which.Should().Be("'Id' must not be empty.");
		response.Template.Should().BeNull();
		handler.Requests.Should().BeEmpty();
	}

	//	============================================================================
	//	Null arguments and exceptions before the request is sent
	//	============================================================================

	[Theory]
	[MemberData(nameof(ClientOperations.AllWithRequest), MemberType = typeof(ClientOperations))]
	public async Task NullRequest_ReturnsInvalid_WithoutHttpCall(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var result = await ClientOperations.InvokeWithNullAsync(docuSeal, operation);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be(operation == ClientOperations.CreateTemplateFromFile
			? "'File' must not be null."
			: "'Request' must not be null.");
		handler.Requests.Should().BeEmpty("a null request must not reach the API");
		ShouldHaveNoPayload(result);
	}

	[Theory]
	[MemberData(nameof(ClientOperations.AllIdOnly), MemberType = typeof(ClientOperations))]
	public async Task DefaultId_ReturnsInvalid_WithoutHttpCall(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var result = await ClientOperations.InvokeWithDefaultIdAsync(docuSeal, operation);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be("'Id' must not be empty.");
		handler.Requests.Should().BeEmpty("an empty id must not reach the API");
		ShouldHaveNoPayload(result);
	}

	[Fact]
	public async Task NullRequest_WithPreCancelledToken_ReturnsCancelled() {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		using var cts = new CancellationTokenSource();

		await cts.CancelAsync();

		var fromRequest = await docuSeal.CreateTemplateAsync((CreateTemplate.Request)null!, cts.Token);
		var fromFile = await docuSeal.CreateTemplateAsync((FileInfo)null!, cts.Token);

		fromRequest.Errors.Should().Equal(_cancelled);
		fromFile.Errors.Should().Equal(_cancelled);
		handler.Requests.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ClientOperations.All), MemberType = typeof(ClientOperations))]
	public async Task ThrowingValidator_ReturnsFailed_WithoutHttpCall(
		string operation) {
		var handler = new CapturingHandler("{}");
		var docuSeal = TestClients.Create(handler, TestClients.UseThrowingValidators);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be(_failed);
		handler.Requests.Should().BeEmpty("an exception while preparing the request must not reach the API");
		ShouldHaveNoPayload(result);
	}

	/// <summary>
	/// On .NET Framework a search string over 32,766 characters makes
	/// <c>Uri.EscapeDataString</c> throw while the endpoint is built. The test
	/// runtime has no such limit, so no public request can make an endpoint
	/// getter throw here; the guard every operation goes through is driven
	/// directly instead, with a send step that throws the way that getter does.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Guard_ExceptionWhileBuildingTheRequest_ReturnsFailed(
		bool throwsSynchronously) {
		var guard = typeof(IDocuSealClient).Assembly
			.GetType("Arex388.DocuSeal.DocuSealClient", throwOnError: true)!
			.GetMethod("GuardAsync", BindingFlags.NonPublic | BindingFlags.Static)!
			.MakeGenericMethod(typeof(ListSubmissions.Request), typeof(ListSubmissions.Response));
		Func<ListSubmissions.Request, CancellationToken, Task<ListSubmissions.Response>> send = throwsSynchronously
			? (_, _) => throw new UriFormatException("Simulated endpoint failure.")
			: (_, _) => Task.FromException<ListSubmissions.Response>(new UriFormatException("Simulated endpoint failure."));

		var response = await (Task<ListSubmissions.Response>)guard.Invoke(null, [
			new ListSubmissions.Request {
				Search = "search"
			},
			new InlineValidator<ListSubmissions.Request>(),
			send,
			CancellationToken.None
		])!;

		response.Success.Should().BeFalse();
		response.Errors.Should().Equal(_failed);
	}

	private static CreateTemplate.Request CreateTemplateRequest(
		FieldType type) => new() {
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
							Name = "Field",
							Role = "First Party",
							Type = type
						}
					],
					FileBase64 = "JVBERi0xLjQK",
					Name = "Test Document"
				}
			],
			Endpoint = CreateTemplate.Endpoints.Pdf,
			Name = "Test Template"
		};

	[Fact]
	public async Task CreateTemplate_CellsField_PassesValidation() {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		await docuSeal.CreateTemplateAsync(CreateTemplateRequest(FieldType.Cells));

		handler.Requests.Should().ContainSingle("a Cells field must pass validation and reach the API");
	}

	[Fact]
	public async Task CreateTemplate_UnknownField_ReturnsInvalid_WithoutHttpCall() {
		var docuSeal = TestClients.CreateWithJson("{}", out var handler);

		var response = await docuSeal.CreateTemplateAsync(CreateTemplateRequest(FieldType.Unknown));

		response.Success.Should().BeFalse();
		response.Errors.Should().ContainSingle().Which.Should().Be("'Type' must not be empty.");
		handler.Requests.Should().BeEmpty();
	}

	[Theory]
	[MemberData(nameof(ClientOperations.All), MemberType = typeof(ClientOperations))]
	public async Task ThrowingHandler_ReturnsFailed(
		string operation) {
		var handler = new ThrowingHandler();
		var docuSeal = TestClients.Create(handler);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		handler.Calls.Should().Be(1, "the request must reach the transport before it fails");
		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be(_failed);
		ShouldHaveNoPayload(result);
	}

	[Theory]
	[MemberData(nameof(ClientOperations.All), MemberType = typeof(ClientOperations))]
	public async Task MalformedBody_ReturnsFailed(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("{ broken", out _);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be(_failed);
	}

	[Theory]
	[MemberData(nameof(ClientOperations.AllWithPayload), MemberType = typeof(ClientOperations))]
	public async Task ErrorBody_ReturnsError_AndNullsPayload(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Not Found" }""", out _);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		result.Success.Should().BeFalse();
		result.Errors.Should().Equal("Not Found");
		ShouldHaveNoPayload(result, "an error body must not surface a half-populated payload");
	}

	[Theory]
	[MemberData(nameof(ErrorBodyRows))]
	public async Task ErrorBody_WithAnyStatus_ReturnsError_AndNullsPayload(
		string operation,
		HttpStatusCode statusCode) {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Not Found" }""", out var handler, statusCode);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		handler.Requests.Should().ContainSingle();
		result.Success.Should().BeFalse();
		result.Errors.Should().Equal("Not Found");
		ShouldHaveNoPayload(result, "an error body must not surface a half-populated payload");
	}

	[Theory]
	[MemberData(nameof(UnusableErrorBodyRows))]
	public async Task NonSuccessStatus_WithoutErrorMember_ReturnsFailed(
		string operation,
		HttpStatusCode statusCode,
		string body) {
		var docuSeal = TestClients.CreateWithJson(body, out var handler, statusCode);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		handler.Requests.Should().ContainSingle();
		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle().Which.Should().Be(_failed);
		ShouldHaveNoPayload(result);
	}

	[Theory]
	[MemberData(nameof(ClientOperations.All), MemberType = typeof(ClientOperations))]
	public async Task Fixtures_ReturnSuccess(
		string operation) {
		var docuSeal = TestClients.CreateWithFixtures();

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		result.Errors.Should().BeEmpty();
		result.Success.Should().BeTrue();

		if (result.Payload is ListPayload list) {
			list.Items.Should().NotBeEmpty();
			list.Pagination.Count.Should().Be(list.Items.Count);
		}
	}

	//	============================================================================
	//	CreateSubmission body shapes
	//	============================================================================

	[Theory]
	[InlineData("[]")]
	[InlineData("{}")]
	[InlineData("""{ "id": 3009, "submission_id": 2009 }""")]
	[InlineData("""{ "error": 42 }""")]
	[InlineData("2001")]
	[InlineData("null")]
	[InlineData("""{ "error": "" }""")]
	[InlineData("""[{ "id": 3001, "submission_id": 2001 }, null]""")]
	public async Task CreateSubmission_UnusableSuccessBody_ReturnsFailed(
		string json) {
		var docuSeal = TestClients.CreateWithJson(json, out _);

		var response = await ClientOperations.InvokeAsync(docuSeal, nameof(IDocuSealClient.CreateSubmissionAsync));

		response.Success.Should().BeFalse();
		response.Errors.Should().ContainSingle().Which.Should().Be(_failed);
		response.Payload.Should().BeNull();
	}

	[Theory]
	[InlineData("[]")]
	[InlineData("{}")]
	[InlineData("""{ "id": 3009, "submission_id": 2009 }""")]
	[InlineData("""{ "error": 42 }""")]
	[InlineData("2001")]
	[InlineData("null")]
	[InlineData("""{ "error": "" }""")]
	[InlineData("""[{ "id": 3001, "submission_id": 2001 }, null]""")]
	public async Task CreateSubmissionFromEmails_UnusableSuccessBody_ReturnsFailed(
		string json) {
		var docuSeal = TestClients.CreateWithJson(json, out _);

		var response = await ClientOperations.InvokeAsync(docuSeal, nameof(IDocuSealClient.CreateSubmissionFromEmailsAsync));

		response.Success.Should().BeFalse();
		response.Errors.Should().ContainSingle().Which.Should().Be(_failed);
		response.Payload.Should().BeNull();
	}

	//	The last error member wins, as with JsonElement.TryGetProperty, even when an earlier one cannot be decoded.
	[Theory]
	[InlineData(nameof(IDocuSealClient.CreateSubmissionAsync), """{ "error": "\uD800", "error": "Template missing" }""")]
	[InlineData(nameof(IDocuSealClient.CreateSubmissionAsync), """{ "error": 42, "error": "Template missing" }""")]
	[InlineData(nameof(IDocuSealClient.CreateSubmissionFromEmailsAsync), """{ "error": "\uD800", "error": "Template missing" }""")]
	[InlineData(nameof(IDocuSealClient.CreateSubmissionFromEmailsAsync), """{ "error": 42, "error": "Template missing" }""")]
	public async Task CreateSubmissionErrorBody_DuplicateError_SurfacesTheLastOccurrence(
		string operation,
		string json) {
		var docuSeal = TestClients.CreateWithJson(json, out _, HttpStatusCode.UnprocessableEntity);

		var response = await ClientOperations.InvokeAsync(docuSeal, operation);

		response.Success.Should().BeFalse();
		response.Errors.Should().Equal("Template missing");
		response.Payload.Should().BeNull();
	}

	//	============================================================================
	//	Canned responses are not shared
	//	============================================================================

	[Fact]
	public async Task Failed_IsAFreshInstance_SoAMutationDoesNotLeak() {
		var docuSeal = TestClients.Create(new ThrowingHandler());

		var first = await docuSeal.GetTemplateAsync(ClientOperations.TemplateId);

		first.Errors.Should().Equal(_failed);
		first.Errors.Clear();
		first.Errors.Add("Mutated by a consumer.");

		var second = await docuSeal.GetTemplateAsync(ClientOperations.TemplateId);

		second.Should().NotBeSameAs(first);
		second.Errors.Should().Equal(_failed);
	}

	[Fact]
	public async Task Cancelled_IsAFreshInstance_SoAMutationDoesNotLeak() {
		var docuSeal = TestClients.CreateWithJson("{}", out _);

		using var cts = new CancellationTokenSource();

		await cts.CancelAsync();

		var first = await docuSeal.GetTemplateAsync(ClientOperations.TemplateId, cts.Token);

		first.Errors.Should().Equal(_cancelled);
		first.Errors.Clear();

		var second = await docuSeal.GetTemplateAsync(ClientOperations.TemplateId, cts.Token);

		second.Should().NotBeSameAs(first);
		second.Errors.Should().Equal(_cancelled);
	}

	/// <summary>
	/// A payload operation's payload is null; a list's is empty with the default
	/// pagination; an operation without a payload has nothing to check.
	/// </summary>
	private static void ShouldHaveNoPayload(
		OperationResult result,
		string because = "") {
		if (result.Payload is ListPayload list) {
			list.Items.Should().BeEmpty(because);
			list.Pagination.Count.Should().Be(0, because);

			return;
		}

		result.Payload.Should().BeNull(because);
	}

	private static OperationResult Shape<TResponse>(
		TResponse response)
		where TResponse : ResponseBase<TResponse>, new() => new(response.Success, response.Errors, null);
}
