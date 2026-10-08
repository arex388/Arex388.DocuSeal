using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A field's display preferences.
/// </summary>
public sealed class FieldPreferences {
	/// <summary>
	/// The horizontal alignment of the field's text value.
	/// </summary>
	public FieldAlign? Align { get; init; }

	/// <summary>
	/// The field box's background color.
	/// </summary>
	public string? Background { get; init; }

	/// <summary>
	/// The font color of the field's value.
	/// </summary>
	public string? Color { get; init; }

	/// <summary>
	/// The currency of a payment field.
	/// </summary>
	public Currency? Currency { get; init; }

	/// <summary>
	/// The font family of the field's value.
	/// </summary>
	public FieldFont? Font { get; init; }

	/// <summary>
	/// The font size of the field's value, in pixels.
	/// </summary>
	[JsonPropertyName("font_size")]
	public int? FontSize { get; init; }

	/// <summary>
	/// The font type of the field's value.
	/// </summary>
	[JsonPropertyName("font_type")]
	public FieldFontType? FontType { get; init; }

	/// <summary>
	/// The data format of the field's value. It depends on the field's type.
	/// </summary>
	public string? Format { get; init; }

	/// <summary>
	/// How the field is masked on the document: <c>true</c> or <c>false</c>, or the number of characters to leave visible.
	/// </summary>
	public JsonElement? Mask { get; init; }

	/// <summary>
	/// The price of a payment field.
	/// </summary>
	public decimal? Price { get; init; }

	/// <summary>
	/// The signature reasons to choose from.
	/// </summary>
	public IList<string>? Reasons { get; init; }

	/// <summary>
	/// The vertical alignment of the field's text value.
	/// </summary>
	[JsonPropertyName("valign")]
	public FieldVerticalAlign? VerticalAlign { get; init; }
}