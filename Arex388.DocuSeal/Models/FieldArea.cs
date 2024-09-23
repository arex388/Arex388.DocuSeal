using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A field area.
/// </summary>
public sealed class FieldArea {
	/// <summary>
	/// The field's attachment id.
	/// </summary>
	[JsonPropertyName("attachment_uuid")]
	public Guid AttachmentId { get; init; }

	/// <summary>
	/// The field's height. The value is from 0.0 to 1.0.
	/// </summary>
	[JsonPropertyName("h")]
	public decimal Height { get; init; }

	/// <summary>
	/// The field's page.
	/// </summary>
	public int Page { get; init; }

	/// <summary>
	/// The field's width. The value is from 0.0 to 1.0.
	/// </summary>
	[JsonPropertyName("w")]
	public decimal Width { get; init; }

	/// <summary>
	/// The field's upper left corner X coordinate. The value is a fraction of the page's width from 0.0 to 1.0.
	/// </summary>
	public decimal X { get; init; }

	/// <summary>
	/// The field's upper left corner Y coordinate. The value is a fraction of the page's height from 0.0 to 1.0.
	/// </summary>
	public decimal Y { get; init; }
}