using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Models;

/// <summary>
/// A schema.
/// </summary>
public sealed class Schema {
	/// <summary>
	/// The schema's attachment id.
	/// </summary>
	[JsonPropertyName("attachment_uuid")]
	public Guid AttachmentId { get; init; }

	/// <summary>
	/// The schema's name.
	/// </summary>
	public string Name { get; init; } = null!;
}