namespace System.Net.Http;

internal static class HttpClientExtensions {
	private const string _authorizationTokenHeader = "X-Auth-Token";

	/// <summary>
	/// Sets the DocuSeal authorization token on the client's default headers,
	/// replacing any earlier value so the client sends exactly one.
	/// </summary>
	public static void SetAuthorizationToken(
		this HttpClient httpClient,
		string authorizationToken) {
		var headers = httpClient.DefaultRequestHeaders;

		headers.Remove(_authorizationTokenHeader);
		headers.Add(_authorizationTokenHeader, authorizationToken);
	}
}