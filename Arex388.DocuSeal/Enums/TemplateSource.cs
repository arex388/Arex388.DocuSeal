namespace Arex388.DocuSeal;

/// <summary>
/// A template source.
/// </summary>
public enum TemplateSource {
	/// <summary>
	/// An unknown template source.
	/// </summary>
	Unknown,

	/// <summary>
	/// Created via the API.
	/// </summary>
	Api,

	/// <summary>
	/// Created via an embedded builder.
	/// </summary>
	Embed,

	/// <summary>
	/// Created via MCP.
	/// </summary>
	Mcp,

	/// <summary>
	/// Created in the DocuSeal application.
	/// </summary>
	Native
}