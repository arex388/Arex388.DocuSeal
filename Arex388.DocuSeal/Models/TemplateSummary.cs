using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A summary of the template a submission was created from.
/// </summary>
public sealed class TemplateSummary {
	/// <summary>
	/// The template's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime CreatedAtUtc { get; init; }

	/// <summary>
	/// The template's external id.
	/// </summary>
	[JsonPropertyName("external_id")]
	public string? ExternalId { get; init; }

	/// <summary>
	/// The template's folder.
	/// </summary>
	[JsonPropertyName("folder_name")]
	public string Folder { get; init; } = null!;

	/// <summary>
	/// The template's id.
	/// </summary>
	public TemplateId Id { get; init; }

	/// <summary>
	/// The template's name.
	/// </summary>
	public string Name { get; init; } = null!;

	/// <summary>
	/// The template's updated timestamp.
	/// </summary>
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAtUtc { get; init; }
}