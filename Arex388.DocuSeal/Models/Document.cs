using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A document.
/// </summary>
public sealed class Document {
	/// <summary>
	/// The document's attachment id.
	/// </summary>
	[JsonPropertyName("uuid")]
	public Guid AttachmentId { get; init; }

	/// <summary>
	/// The document's id.
	/// </summary>
	public DocumentId Id { get; init; }

	/// <summary>
	/// The document's name.
	/// </summary>
	[JsonPropertyName("filename")]
	public string Name { get; init; } = null!;

	/// <summary>
	/// The document's preview URL.
	/// </summary>
	[JsonPropertyName("preview_image_url")]
	public Uri PreviewUri { get; init; } = null!;

	/// <summary>
	/// The document's URL.
	/// </summary>
	public Uri Url { get; init; } = null!;
}