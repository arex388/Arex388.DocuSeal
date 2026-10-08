namespace Arex388.DocuSeal;

/// <summary>
/// A completed or signed document of a submission or a submitter.
/// </summary>
public sealed class SubmissionDocument {
	/// <summary>
	/// The document's name.
	/// </summary>
	public string Name { get; init; } = null!;

	/// <summary>
	/// The document's URL.
	/// </summary>
	public Uri Url { get; init; } = null!;
}