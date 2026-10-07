using System.Net;
using System.Text;

namespace Arex388.DocuSeal.Testing;

/// <summary>
/// Mock HTTP handler that returns canned JSON responses from the shared
/// <c>TestData/Responses</c> fixtures, routed by method and path. Shared by the
/// unit tests and the benchmarks so neither ever hits the live DocuSeal API.
/// An unrouted request throws, so a mistyped route surfaces as a Failed response
/// instead of a body that looks like a real API "Not Found". The <c>error.json</c>
/// fixture is for tests that inject an error body explicitly.
/// </summary>
internal sealed class MockHttpMessageHandler :
	HttpMessageHandler {
	private readonly Dictionary<string, string> _responseCache = new(StringComparer.Ordinal);
	private readonly string _responsesDir;

	public MockHttpMessageHandler() {
		_responsesDir = Path.Combine(AppContext.BaseDirectory, "Responses");

		if (!Directory.Exists(_responsesDir)) {
			throw new DirectoryNotFoundException($"Responses directory not found at: {_responsesDir}. Ensure the TestData/Responses fixtures are linked into the project.");
		}

		LoadResponses();
	}

	private void LoadResponses() {
		foreach (var file in Directory.GetFiles(_responsesDir, "*.json")) {
			var fileName = Path.GetFileName(file);
			var content = File.ReadAllText(file);

			_responseCache[fileName] = content;
		}

		if (_responseCache.Count == 0) {
			throw new InvalidOperationException($"No JSON response files found in: {_responsesDir}. Ensure the TestData/Responses fixtures are linked into the project.");
		}
	}

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken) {
		var path = request.RequestUri?.AbsolutePath.Trim('/') ?? string.Empty;
		var fileName = GetResponseFileName(request.Method, path);

		if (fileName is null) {
			throw new InvalidOperationException($"No fixture route for {request.Method} /{path}. Add it to {nameof(MockHttpMessageHandler)}.{nameof(GetResponseFileName)}.");
		}

		if (!_responseCache.TryGetValue(fileName, out var json)) {
			throw new InvalidOperationException($"Fixture '{fileName}' for {request.Method} /{path} was not found in: {_responsesDir}.");
		}

		return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
			Content = new StringContent(json, Encoding.UTF8, "application/json"),
			RequestMessage = request
		});
	}

	private static string? GetResponseFileName(
		HttpMethod method,
		string path) {
		var segments = path.Split('/');

		return segments switch {
			["templates"] when method == HttpMethod.Get => "templates.json",
			["templates", "pdf" or "docx" or "merge"] when method == HttpMethod.Post => "template.json",
			["templates", _] when method == HttpMethod.Get
								  || method == HttpMethod.Put
								  || method == HttpMethod.Delete => "template.json",
			["templates", _, "clone"] when method == HttpMethod.Post => "template.json",
			["templates", _, "documents"] when method == HttpMethod.Put => "template.json",
			["submissions"] when method == HttpMethod.Get => "submissions.json",
			["submissions"] when method == HttpMethod.Post => "submissions-created.json",
			["submissions", _] when method == HttpMethod.Get
									|| method == HttpMethod.Delete => "submission.json",
			["submitters"] when method == HttpMethod.Get => "submitters.json",
			["submitters", _] when method == HttpMethod.Get
								   || method == HttpMethod.Put => "submitter.json",
			_ => null
		};
	}
}
