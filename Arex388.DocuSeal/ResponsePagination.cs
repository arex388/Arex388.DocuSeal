using System.Text.Json.Serialization;

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

	/// <summary>
	/// The id to pass as <c>after</c> to load the next page, or <see langword="null" /> when there is none.
	/// </summary>
	public int? Next { get; init; }

	/// <summary>
	/// The id to pass as <c>before</c> to load the previous page, or <see langword="null" /> when there is none.
	/// </summary>
	[JsonPropertyName("prev")]
	public int? Previous { get; init; }
}