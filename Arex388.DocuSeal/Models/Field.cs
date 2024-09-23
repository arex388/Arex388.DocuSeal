using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A field.
/// </summary>
public sealed class Field {
	/// <summary>
	/// The field's areas.
	/// </summary>
	public IList<FieldArea> Areas { get; init; } = [];

	/// <summary>
	/// The field's id.
	/// </summary>
	[JsonPropertyName("uuid")]
	public Guid Id { get; init; }

	/// <summary>
	/// Flag indicating if the field is required.
	/// </summary>
	[JsonPropertyName("required")]
	public bool IsRequired { get; init; }

	/// <summary>
	/// The field's name.
	/// </summary>
	public string? Name { get; init; }

	/// <summary>
	/// The field's submitter id.
	/// </summary>
	[JsonPropertyName("submitter_uuid")]
	public Guid SubmitterId { get; init; }

	/// <summary>
	/// The field's type.
	/// </summary>
	//[JsonConverter(typeof(FieldTypeJsonConverter))]
	public FieldType Type { get; init; }
}