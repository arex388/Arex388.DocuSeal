namespace Arex388.DocuSeal;

/// <summary>
/// DocuSeal API configuration options.
/// </summary>
public sealed class DocuSealClientOptions {
	/// <summary>
	/// The API authorization token.
	/// </summary>
	public required string AuthorizationToken { get; init; }

	/// <summary>
	/// The region the account lives in, which decides the API host. Defaults to <see cref="DocuSealRegion.Global"/>.
	/// </summary>
	public DocuSealRegion Region { get; init; } = DocuSealRegion.Global;
}