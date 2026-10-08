namespace Arex388.DocuSeal;

/// <summary>
/// The DocuSeal region an account lives in, which decides the API host.
/// </summary>
public enum DocuSealRegion {
	/// <summary>
	/// The global region, served from <c>https://api.docuseal.com</c>.
	/// </summary>
	Global,

	/// <summary>
	/// The EU region, served from <c>https://api.docuseal.eu</c>.
	/// </summary>
	Eu
}