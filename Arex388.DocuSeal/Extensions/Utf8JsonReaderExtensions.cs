namespace System.Text.Json;

internal static class Utf8JsonReaderExtensions {
	/// <summary>
	/// The enum converters' result when the current token matched none of their
	/// tokens. The converters match a string token with
	/// <see cref="Utf8JsonReader.ValueTextEquals(ReadOnlySpan{byte})"/>, which does
	/// not allocate; only an unmatched token reaches this method, which still calls
	/// <c>GetString()</c> so the outcome is what it was when every read began with
	/// <c>GetString()</c>: a null token or an unknown string returns
	/// <paramref name="unknown"/>, and a token that is not a string, or a string
	/// that is not valid UTF-8, throws as before.
	/// </summary>
	public static T Unmatched<T>(
		this ref Utf8JsonReader reader,
		T unknown) {
		_ = reader.GetString();

		return unknown;
	}
}
