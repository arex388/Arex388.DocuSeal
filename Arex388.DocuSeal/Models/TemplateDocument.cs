using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A document attached to a template.
/// </summary>
public sealed class TemplateDocument {
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
	/// The document's name. <see langword="null" /> on the <c>CreateTemplateFromHtmlAsync</c> and <c>UpdateTemplateDocumentsAsync</c> responses, which do not carry it.
	/// </summary>
	[JsonPropertyName("filename")]
	public string? Name { get; init; }

	/// <summary>
	/// The document's preview URL. <see langword="null" /> on the <c>CreateTemplateFromHtmlAsync</c> and <c>UpdateTemplateDocumentsAsync</c> responses, which do not carry it.
	/// </summary>
	[JsonPropertyName("preview_image_url")]
	public Uri? PreviewUri { get; init; }

	/// <summary>
	/// The document's URL.
	/// </summary>
	public Uri Url { get; init; } = null!;
}