namespace Arex388.DocuSeal;

internal static class HttpClientHelper {
	private static readonly Uri _eu = new("https://api.docuseal.eu/");
	private static readonly Uri _global = new("https://api.docuseal.com/");

	public static Uri GetBaseAddress(
		DocuSealRegion region) => region switch {
			DocuSealRegion.Global => _global,
			DocuSealRegion.Eu => _eu,
			_ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown DocuSeal region.")
		};
}