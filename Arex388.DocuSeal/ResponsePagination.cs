namespace Arex388.DocuSeal;

/// <summary>
/// The response's pagination details.
/// </summary>
public sealed class ResponsePagination {
	internal static readonly ResponsePagination Empty = new();

	/// <summary>
	/// The number of results returned.
	/// </summary>
	public int Count { get; init; }
}