namespace Arex388.DocuSeal.Models;

internal static class ErrableExtensions {
	public static bool HasErred(
		this IErrable? errable) => errable?.Error.HasValue()
								   ?? false;
}