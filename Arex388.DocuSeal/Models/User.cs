using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Models;

/// <summary>
/// A user.
/// </summary>
public sealed class User {
	/// <summary>
	/// The user's email/
	/// </summary>
	public string Email { get; init; } = null!;

	/// <summary>
	/// The user's first name.
	/// </summary>
	[JsonPropertyName("first_name")]
	public string FirstName { get; init; } = null!;

	/// <summary>
	/// The user's id.
	/// </summary>
	public UserId Id { get; init; }

	/// <summary>
	/// The user's last name.
	/// </summary>
	[JsonPropertyName("last_name")]
	public string LastName { get; init; } = null!;
}