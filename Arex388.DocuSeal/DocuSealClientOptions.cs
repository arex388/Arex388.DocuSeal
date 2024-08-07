namespace Arex388.DocuSeal;

/// <summary>
/// DocuSeal.co API configuration options.
/// </summary>
public sealed class DocuSealClientOptions {
	/// <summary>
	/// The API authorization token.
	/// </summary>
	public required string AuthorizationToken { get; init; }
}