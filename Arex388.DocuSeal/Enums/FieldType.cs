namespace Arex388.DocuSeal;

/// <summary>
/// A field type.
/// </summary>
public enum FieldType {
	/// <summary>
	/// An unknown field type.
	/// </summary>
	Unknown,

	/// <summary>
	/// A list of cells.
	/// </summary>
	Cells,

	/// <summary>
	/// A checkbox.
	/// </summary>
	Checkbox,

	/// <summary>
	/// A date.
	/// </summary>
	Date,

	/// <summary>
	/// A file.
	/// </summary>
	File,

	/// <summary>
	/// A heading.
	/// </summary>
	Heading,

	/// <summary>
	/// An image.
	/// </summary>
	Image,

	/// <summary>
	/// Initials.
	/// </summary>
	Initials,

	/// <summary>
	/// A knowledge-based authentication (KBA) field.
	/// </summary>
	Kba,

	/// <summary>
	/// Multiple.
	/// </summary>
	Multiple,

	/// <summary>
	/// A number.
	/// </summary>
	Number,

	/// <summary>
	/// A payment.
	/// </summary>
	Payment,

	/// <summary>
	/// A phone.
	/// </summary>
	Phone,

	/// <summary>
	/// A radio selection.
	/// </summary>
	Radio,

	/// <summary>
	/// A selection.
	/// </summary>
	Select,

	/// <summary>
	/// A signature.
	/// </summary>
	Signature,

	/// <summary>
	/// A stamp.
	/// </summary>
	Stamp,

	/// <summary>
	/// A strikethrough.
	/// </summary>
	Strikethrough,

	/// <summary>
	/// Text.
	/// </summary>
	Text,

	/// <summary>
	/// A verification.
	/// </summary>
	Verification
}