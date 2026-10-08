using FluentAssertions;
using System.Net;

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
		.. ClientOperations.WithoutPayload,
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
	[MemberData(nameof(ClientOperations.AllWithoutPayload), MemberType = typeof(ClientOperations))]
	public async Task ErrorBody_ReturnsError(
		string operation) {
		var docuSeal = TestClients.CreateWithJson("""{ "error": "Not Found" }""", out _);

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		result.Success.Should().BeFalse();
		result.Errors.Should().Equal("Not Found");
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
	//	CreateSubmission array-vs-object fallback
	//	============================================================================

	[Fact]
	public async Task CreateSubmission_ArrayBody_TakesTheFirstSubmitterShapedObject() {
		var docuSeal = TestClients.CreateWithFixtures();

		var response = await ClientOperations.InvokeAsync(docuSeal, nameof(IDocuSealClient.CreateSubmissionAsync));

		response.Success.Should().BeTrue();

		var submission = response.Payload.Should().BeOfType<Submission>().Subject;

		//	The create endpoint returns submitters; `id` is the submitter and
		//	`submission_id` is the submission, which is what Submission.Id maps.
		submission.Id.Should().Be(ClientOperations.SubmissionId);
		submission.Email.Should().Be("signer1@example.com");
		submission.Status.Should().Be(SubmitterStatus.Sent);
	}

	[Fact]
	public async Task CreateSubmission_ObjectBody_FallsBackToASingleSubmission() {
		const string json = """
			{
				"id": 3009,
				"submission_id": 2009,
				"email": "signer9@example.com",
				"status": "pending",
				"created_at": "2024-08-05T15:30:00.000Z",
				"updated_at": "2024-08-05T15:30:00.000Z"
			}
			""";

		var docuSeal = TestClients.CreateWithJson(json, out _);

		var response = await ClientOperations.InvokeAsync(docuSeal, nameof(IDocuSealClient.CreateSubmissionAsync));

		response.Success.Should().BeTrue();

		var submission = response.Payload.Should().BeOfType<Submission>().Subject;

		submission.Id.Should().Be(new SubmissionId(2009));
		submission.Email.Should().Be("signer9@example.com");
		submission.Status.Should().Be(SubmitterStatus.Pending);
	}

	[Fact]
	public async Task CreateSubmission_EmptyArrayBody_ReturnsFailed() {
		var docuSeal = TestClients.CreateWithJson("[]", out _);

		var response = await ClientOperations.InvokeAsync(docuSeal, nameof(IDocuSealClient.CreateSubmissionAsync));

		response.Success.Should().BeFalse();
		response.Errors.Should().ContainSingle().Which.Should().Be(_failed);
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
