namespace Arex388.DocuSeal;

/// <summary>
/// A submitter role defined by a template.
/// </summary>
public sealed class TemplateSubmitter {
	/// <summary>
	/// The submitter's name.
	/// </summary>
	public string Name { get; init; } = null!;

	/// <summary>
	/// The submitter's uuid. It matches the submitter id of the template's fields.
	/// </summary>
	public Guid Uuid { get; init; }
}