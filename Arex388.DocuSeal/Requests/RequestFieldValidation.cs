using FluentValidation;

namespace Arex388.DocuSeal;

/// <summary>
/// A request field's validation rules.
/// </summary>
public sealed class RequestFieldValidation {
	/// <summary>
	/// The maximum allowed value, depending on the field's type: a number for a number field, or a date string for a date field.
	/// </summary>
	/// <remarks>
	/// Only a <see cref="string" /> or a numeric value is accepted.
	/// </remarks>
	public object? Max { get; init; }

	/// <summary>
	/// A custom error message to display on validation failure.
	/// </summary>
	public string? Message { get; init; }

	/// <summary>
	/// The minimum allowed value, depending on the field's type: a number for a number field, or a date string for a date field.
	/// </summary>
	/// <remarks>
	/// Only a <see cref="string" /> or a numeric value is accepted.
	/// </remarks>
	public object? Min { get; init; }

	/// <summary>
	/// An HTML validation pattern, based on the <c>pattern</c> attribute specification (e.g., <c>[A-Z]{4}</c>).
	/// </summary>
	public string? Pattern { get; init; }

	/// <summary>
	/// The increment step for a number field. Pass 1 to accept only integers, or 0.01 to accept decimal currency.
	/// </summary>
	public decimal? Step { get; init; }
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestFieldValidationValidator :
	AbstractValidator<RequestFieldValidation> {
	public RequestFieldValidationValidator() {
		RuleFor(r => r.Max).Must(IsNumberOrString).WithMessage("'{PropertyName}' must be a number or a string.");
		RuleFor(r => r.Min).Must(IsNumberOrString).WithMessage("'{PropertyName}' must be a number or a string.");
	}

	private static bool IsNumberOrString(
		object? value) => value switch {
			null or string or byte or sbyte or short or ushort or int or uint or long or ulong or decimal => true,
			float f => !float.IsNaN(f) && !float.IsInfinity(f),
			double d => !double.IsNaN(d) && !double.IsInfinity(d),
			_ => false
		};
}