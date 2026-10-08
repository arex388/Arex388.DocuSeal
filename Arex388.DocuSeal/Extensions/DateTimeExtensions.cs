using System.Globalization;

namespace System;

internal static class DateTimeExtensions {
	//	An unspecified kind is taken to already be UTC; the members that call this are all named *Utc.
	public static DateTime AsUtc(
		this DateTime value) => value.Kind switch {
			DateTimeKind.Local => value.ToUniversalTime(),
			DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
			_ => value
		};

	public static string ToIso8601String(
		this DateTime value) => value.AsUtc().ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", CultureInfo.InvariantCulture);
}