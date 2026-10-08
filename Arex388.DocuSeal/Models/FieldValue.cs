using System.Text.Json;

namespace Arex388.DocuSeal;

/// <summary>
/// A field's value.
/// </summary>
public sealed class FieldValue {
	/// <summary>
	/// The name of the template field.
	/// </summary>
	public string Field { get; init; } = null!;

	/// <summary>
	/// The field's value. It is a string, number, boolean or array, or <see langword="null" /> when the field is empty.
	/// </summary>
	public JsonElement? Value { get; init; }
}