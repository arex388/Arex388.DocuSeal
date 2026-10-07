using FluentAssertions;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// The no-throw contract: every IDocuSealClient member reports cancellation,
/// validation failures, transport failures, and API error bodies through
/// <c>Errors</c> / <c>Success</c> instead of throwing.
/// </summary>
public sealed class ResponseContractTests {
	private const string _cancelled = "The request was cancelled.";
	private const string _failed = "The request has failed.";

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
		result.Payload.Should().BeNull();
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
		result.Payload.Should().BeNull("an error body must not surface a half-populated payload");
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
	[MemberData(nameof(ClientOperations.All), MemberType = typeof(ClientOperations))]
	public async Task Fixtures_ReturnSuccess(
		string operation) {
		var docuSeal = TestClients.CreateWithFixtures();

		var result = await ClientOperations.InvokeAsync(docuSeal, operation);

		result.Errors.Should().BeEmpty();
		result.Success.Should().BeTrue();
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

	private static OperationResult Shape<TResponse>(
		TResponse response)
		where TResponse : ResponseBase<TResponse>, new() => new(response.Success, response.Errors, null);
}
