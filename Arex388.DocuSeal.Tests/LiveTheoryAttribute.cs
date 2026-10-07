using System.Runtime.CompilerServices;

namespace Arex388.DocuSeal.Tests;

/// <summary>
/// The <c>[Theory]</c> counterpart of <see cref="LiveFactAttribute"/>: every data
/// row is skipped unless DOCUSEAL_LIVE_TESTS=1 is set and the authorizationToken1
/// user secret is present.
/// </summary>
public sealed class LiveTheoryAttribute :
	TheoryAttribute {
	public LiveTheoryAttribute(
		[CallerFilePath] string? sourceFilePath = null,
		[CallerLineNumber] int sourceLineNumber = -1) :
		base(sourceFilePath, sourceLineNumber) {
		Skip = LiveTests.SkipReason;
	}
}
