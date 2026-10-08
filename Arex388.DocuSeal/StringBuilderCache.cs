using System.Text;

namespace Arex388.DocuSeal;

/// <summary>
/// One reusable <see cref="StringBuilder"/> per thread for building short strings
/// such as list endpoints, so building one allocates only the resulting string.
/// A builder that grew past <see cref="MaxCapacity"/> is not kept.
/// </summary>
internal static class StringBuilderCache {
	private const int MaxCapacity = 512;

	[ThreadStatic]
	private static StringBuilder? _cached;

	public static StringBuilder Acquire() {
		var builder = _cached;

		if (builder is null) {
			return new StringBuilder(128);
		}

		_cached = null;

		return builder.Clear();
	}

	public static string GetStringAndRelease(
		StringBuilder builder) {
		var value = builder.ToString();

		if (builder.Capacity <= MaxCapacity) {
			_cached = builder;
		}

		return value;
	}
}
