using FluentValidation;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

/// <summary>
/// A request field's display preferences.
/// </summary>
public sealed class RequestFieldPreferences {
	/// <summary>
	/// The horizontal alignment of the field's text value. The API defaults to <see cref="FieldAlign.Left" />.
	/// </summary>
	public FieldAlign? Align { get; init; }

	/// <summary>
	/// The field box's background color (e.g., <c>white</c> or <c>#FF0000</c>).
	/// </summary>
	public string? Background { get; init; }

	/// <summary>
	/// The font color of the field's value (e.g., <c>black</c> or <c>#FF0000</c>). The API defaults to <c>black</c>.
	/// </summary>
	public string? Color { get; init; }

	/// <summary>
	/// The currency of a payment field. Only for payment fields. The API defaults to <see cref="Currency.Usd" />.
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
	/// The data format of the field's value. It depends on the field's type: a date format such as <c>DD/MM/YYYY</c> for a date field; <c>drawn</c>, <c>typed</c>, <c>drawn_or_typed</c> or <c>upload</c> for a signature field; a currency format such as <c>usd</c> for a number field.
	/// </summary>
	public string? Format { get; init; }

	/// <summary>
	/// How sensitive data is masked on the document: <see langword="true" /> or <see langword="false" />, or the number of characters to leave visible. The API defaults to <see langword="false" />.
	/// </summary>
	/// <remarks>
	/// Only a <see cref="bool" /> or an integer value is accepted.
	/// </remarks>
	public object? Mask { get; init; }

	/// <summary>
	/// The price of a payment field. Only for payment fields.
	/// </summary>
	public decimal? Price { get; init; }

	/// <summary>
	/// The signature reasons to choose from.
	/// </summary>
	public IList<string>? Reasons { get; init; }

	/// <summary>
	/// The vertical alignment of the field's text value. The API defaults to <see cref="FieldVerticalAlign.Center" />.
	/// </summary>
	[JsonPropertyName("valign")]
	public FieldVerticalAlign? VerticalAlign { get; init; }
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestFieldPreferencesValidator :
	AbstractValidator<RequestFieldPreferences> {
	public RequestFieldPreferencesValidator() {
		RuleFor(r => r.Align).Must(a => a != FieldAlign.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
		RuleFor(r => r.Currency).Must(c => c != Currency.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
		RuleFor(r => r.Font).Must(f => f != FieldFont.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
		RuleFor(r => r.FontSize).GreaterThan(0).When(r => r.FontSize.HasValue);
		RuleFor(r => r.FontType).Must(t => t != FieldFontType.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
		RuleFor(r => r.Mask).Must(IsBoolOrInteger).WithMessage("'{PropertyName}' must be a boolean or an integer.");
		RuleFor(r => r.VerticalAlign).Must(a => a != FieldVerticalAlign.Unknown).WithMessage("'{PropertyName}' must not be unknown.");
	}

	private static bool IsBoolOrInteger(
		object? value) => value is null or bool or byte or sbyte or short or ushort or int or uint or long or ulong;
}