namespace Arex388.DocuSeal;

/// <summary>
/// DocuSeal.co API client factory for interacting with multiple accounts.
/// </summary>
public interface IDocuSealClientFactory {
	/// <summary>
	/// Create and cache an instance of the DocuSeal.co API client.
	/// </summary>
	/// <param name="options">The client's configuration options.</param>
	/// <returns>An instance of the client.</returns>
	IDocuSealClient CreateClient(
		DocuSealClientOptions options);
}