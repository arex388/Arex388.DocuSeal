namespace Arex388.DocuSeal;

/// <summary>
/// DocuSeal API client factory for interacting with multiple accounts.
/// </summary>
public interface IDocuSealClientFactory {
	/// <summary>
	/// Create and cache an instance of the DocuSeal API client.
	/// </summary>
	/// <remarks>
	/// Clients are cached per authorization token and region, so the same token in two regions yields two clients.
	/// </remarks>
	/// <param name="options">The client's configuration options.</param>
	/// <returns>An instance of the client.</returns>
	IDocuSealClient CreateClient(
		DocuSealClientOptions options);
}