using FluentValidation.Results;

namespace Arex388.DocuSeal;

/// <summary>
/// The response's base details.
/// </summary>
/// <typeparam name="TResponse">The response's type.</typeparam>
public abstract class ResponseBase<TResponse>
	where TResponse : ResponseBase<TResponse>, new() {
	/// <summary>
	/// The request's errors, if any.
	/// </summary>
	public IList<string> Errors { get; internal set; } = [];

	/// <summary>
	/// The request's status.
	/// </summary>
	public bool Success => Errors.Count == 0;

	//	============================================================================
	//	Responses
	//	============================================================================

	//	Cancelled and Failed return a new instance per access: Errors is a mutable
	//	list, so a shared instance would let one caller's edit leak into every
	//	later response of that kind.
	internal static TResponse Cancelled => new() {
		Errors = [
			"The request was cancelled."
		]
	};
	internal static TResponse Failed => new() {
		Errors = [
			"The request has failed."
		]
	};
	internal static TResponse Invalid(
		ValidationResult validationResult) => new() {
			Errors = validationResult.ToErrors()
		};
}