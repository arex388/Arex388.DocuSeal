using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A template.
/// </summary>
public sealed class Template {
	/// <summary>
	/// The template's archived timestamp.
	/// </summary>
	[JsonPropertyName("archived_at")]
	public DateTime? ArchivedAtUtc { get; init; }

	/// <summary>
	/// The template's author.
	/// </summary>
	public User Author { get; init; } = null!;

	/// <summary>
	/// The template's author id.
	/// </summary>
	[JsonPropertyName("author_id")]
	public UserId AuthorId { get; init; }

	/// <summary>
	/// The template's created timestamp.
	/// </summary>
	[JsonPropertyName("created_at")]
	public DateTime CreatedAtUtc { get; init; }

	/// <summary>
	/// The template's documents.
	/// </summary>
	public IList<TemplateDocument> Documents { get; init; } = [];

	[JsonInclude]
	internal string? Error { get; init; }

	/// <summary>
	/// The template's external id.
	/// </summary>
	[JsonPropertyName("external_id")]
	public string? ExternalId { get; init; }

	/// <summary>
	/// The template's fields.
	/// </summary>
	public IList<Field> Fields { get; init; } = [];

	/// <summary>
	/// The template's folder id.
	/// </summary>
	[JsonPropertyName("folder_id")]
	public FolderId? FolderId { get; init; }

	/// <summary>
	/// The template's folder.
	/// </summary>
	[JsonPropertyName("folder_name")]
	public string Folder { get; init; } = null!;

	/// <summary>
	/// Flag indicating if the template is accessible by link.
	/// </summary>
	[JsonPropertyName("shared_link")]
	public bool HasSharedLink { get; init; }

	/// <summary>
	/// The template's id.
	/// </summary>
	public TemplateId Id { get; init; }

	/// <summary>
	/// The template's name.
	/// </summary>
	public string Name { get; init; } = null!;

	/// <summary>
	/// The template's preferences. The object is free-form.
	/// </summary>
	public JsonObject? Preferences { get; init; }

	/// <summary>
	/// The template's schemas.
	/// </summary>
	[JsonPropertyName("schema")]
	public IList<Schema> Schemas { get; init; } = [];

	/// <summary>
	/// The template's submitters.
	/// </summary>
	public IList<TemplateSubmitter> Submitters { get; init; } = [];

	/// <summary>
	/// The template's slug.
	/// </summary>
	public string Slug { get; init; } = null!;

	/// <summary>
	/// The template's source.
	/// </summary>
	public TemplateSource Source { get; init; }

	/// <summary>
	/// The template's updated timestamp.
	/// </summary>
	[JsonPropertyName("updated_at")]
	public DateTime UpdatedAtUtc { get; init; }

	/// <summary>
	/// The schema of the template's dynamic document content variables. The object is free-form.
	/// </summary>
	[JsonPropertyName("variables_schema")]
	public JsonObject? VariablesSchema { get; init; }
}