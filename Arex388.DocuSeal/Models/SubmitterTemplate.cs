using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// The base details of the template a submitter signs.
/// </summary>
public sealed class SubmitterTemplate {
	/// <summary>
	/// The template's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime CreatedAtUtc { get; init; }

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