using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Arex388.DocuSeal.Tests.Unit;

/// <summary>
/// Reads response bodies served as raw bytes, with or without a charset and a
/// byte order mark. A valid UTF-8 body binds from its bytes; any other charset,
/// a UTF-16 byte order mark, or bytes that are not valid UTF-8 are decoded as
/// <c>ReadAsStringAsync</c> decodes them, so every shape binds the same model and
/// invalid bytes read as U+FFFD, even in members bound lazily.
/// </summary>
public sealed class ResponseEncodingTests {
	private const string Replacement = "\uFFFD";

	private static readonly string _submitter = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Responses", "submitter.json"));

	private static async Task<GetSubmitter.Response> GetSubmitterAsync(
		byte[] body,
		string? contentType) {
		var docuSeal = TestClients.Create(new BytesHandler(body, contentType));

		return await docuSeal.GetSubmitterAsync(ClientOperations.SubmitterId);
	}

	private static string Bound(
		GetSubmitter.Response response) => JsonSerializer.Serialize(response.Submitter, TestClients.JsonOptions);

	//	The UTF-8 bytes of the JSON, with each '~' replaced by the invalid byte 0xFF.
	private static byte[] WithInvalidByte(
		string json) => [.. Encoding.UTF8.GetBytes(json).Select(b => b == (byte)'~' ? (byte)0xFF : b)];

	//	============================================================================
	//	Charsets and byte order marks
	//	============================================================================

	public static TheoryData<string, string?> EncodedBodies => new() {
		{ "utf-8", null },
		{ "utf-8", "application/json" },
		{ "utf-8", "application/json; charset=utf-8" },
		{ "utf-8", "application/json; charset=UTF-8" },
		{ "utf-8", "application/json; charset=\"utf-8\"" },
		{ "utf-8-bom", null },
		{ "utf-8-bom", "application/json; charset=utf-8" },
		{ "utf-16le-bom", null },
		{ "utf-16be-bom", null },
		{ "utf-16le", "application/json; charset=utf-16" },
		{ "latin-1", "application/json; charset=iso-8859-1" }
	};

	[Theory]
	[MemberData(nameof(EncodedBodies))]
	public async Task Body_BindsTheSameModelAsAPlainUtf8Body(
		string encoding,
		string? contentType) {
		byte[] body = encoding switch {
			"utf-8" => Encoding.UTF8.GetBytes(_submitter),
			"utf-8-bom" => [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(_submitter)],
			"utf-16le-bom" => [.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes(_submitter)],
			"utf-16be-bom" => [.. Encoding.BigEndianUnicode.GetPreamble(), .. Encoding.BigEndianUnicode.GetBytes(_submitter)],
			"utf-16le" => Encoding.Unicode.GetBytes(_submitter),
			"latin-1" => Encoding.Latin1.GetBytes(_submitter),
			_ => throw new ArgumentOutOfRangeException(nameof(encoding), encoding, "Unknown encoding.")
		};
		var expected = Bound(await GetSubmitterAsync(Encoding.UTF8.GetBytes(_submitter), null));

		var response = await GetSubmitterAsync(body, contentType);

		response.Success.Should().BeTrue();
		response.Submitter.Should().NotBeNull();
		Bound(response).Should().Be(expected);
	}

	//	============================================================================
	//	Invalid UTF-8
	//	============================================================================

	[Theory]
	[InlineData(null)]
	[InlineData("application/json; charset=utf-8")]
	public async Task InvalidUtf8_InFreeFormValues_ReadsAsReplacementCharacter(
		string? contentType) {
		var body = WithInvalidByte("""{ "id": 3001, "submission_id": 2001, "metadata": { "note": "a~b" }, "values": [{ "field": "Full Name", "value": "a~b" }] }""");

		var response = await GetSubmitterAsync(body, contentType);

		response.Success.Should().BeTrue();
		response.Submitter!.Metadata!["note"]!.GetValue<string>().Should().Be($"a{Replacement}b");
		response.Submitter.Values.Should().ContainSingle().Which.Value!.Value.GetString().Should().Be($"a{Replacement}b");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("application/json; charset=utf-8")]
	public async Task InvalidUtf8_InFreeFormPropertyNames_ReadsAsReplacementCharacter(
		string? contentType) {
		var body = WithInvalidByte("""{ "id": 3001, "submission_id": 2001, "metadata": { "n~": "x" }, "values": [{ "field": "Full Name", "value": { "k~": 1 } }] }""");

		var response = await GetSubmitterAsync(body, contentType);

		response.Success.Should().BeTrue();
		response.Submitter!.Metadata!.ContainsKey($"n{Replacement}").Should().BeTrue();
		response.Submitter.Metadata[$"n{Replacement}"]!.GetValue<string>().Should().Be("x");
		response.Submitter.Values.Should().ContainSingle().Which.Value!.Value.GetProperty($"k{Replacement}").GetInt32().Should().Be(1);
	}

	[Fact]
	public async Task InvalidUtf8_InABoundString_ReadsAsReplacementCharacter() {
		var body = WithInvalidByte("""{ "id": 3001, "submission_id": 2001, "name": "Signer~One" }""");

		var response = await GetSubmitterAsync(body, null);

		response.Success.Should().BeTrue();
		response.Submitter!.Name.Should().Be($"Signer{Replacement}One");
	}
}

/// <summary>
/// Serves the same bytes for every request with the given <c>Content-Type</c>,
/// or none, so a test controls the charset and byte order mark exactly.
/// </summary>
file sealed class BytesHandler(
	byte[] body,
	string? contentType) :
	HttpMessageHandler {
	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken) {
		var content = new ByteArrayContent(body);

		if (contentType is not null) {
			content.Headers.TryAddWithoutValidation("Content-Type", contentType);
		}

		return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
			Content = content,
			RequestMessage = request
		});
	}
}