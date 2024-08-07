namespace System.Threading;

internal static class CancellationTokenExtensions {
	public static bool IsSupportedAndCancelled(
		this CancellationToken cancellationToken) => cancellationToken.CanBeCanceled
													 // ReSharper disable once MergeIntoPattern
													 && cancellationToken.IsCancellationRequested;
}