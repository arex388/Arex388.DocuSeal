﻿using FluentValidation.Results;

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
	public IList<string> Errors { get; init; } = [];

	/// <summary>
	/// The request's status.
	/// </summary>
	public bool Success => Errors.Count == 0;

	//	============================================================================
	//	Responses
	//	============================================================================

	internal static readonly TResponse Cancelled = new() {
		Errors = [
			"The request was cancelled."
		]
	};
	internal static readonly TResponse Failed = new() {
		Errors = [
			"The request has failed."
		]
	};
	internal static TResponse Invalid(
		ValidationResult validationResult) => new() {
			Errors = validationResult.ToErrors()
		};
}