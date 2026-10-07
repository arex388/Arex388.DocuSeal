using System.Runtime.CompilerServices;

namespace Arex388.DocuSeal.Tests;

/// <summary>
/// A fact that runs against the live DocuSeal API, spends real quota, and creates
/// real templates and submissions on the account. Skipped unless the
/// DOCUSEAL_LIVE_TESTS environment variable is set to "1" AND the
/// authorizationToken1 user secret is present — a bare test run never hits the API.
/// </summary>
public sealed class LiveFactAttribute :
	FactAttribute {
	public LiveFactAttribute(
		[CallerFilePath] string? sourceFilePath = null,
		[CallerLineNumber] int sourceLineNumber = -1) :
		base(sourceFilePath, sourceLineNumber) {
		Skip = LiveTests.SkipReason;
	}
}

/// <summary>
/// The shared opt-in gate for <see cref="LiveFactAttribute"/> and
/// <see cref="LiveTheoryAttribute"/>.
/// </summary>
internal static class LiveTests {
	public const string EnvironmentVariable = "DOCUSEAL_LIVE_TESTS";

	/// <summary>
	/// <c>null</c> when the live tests should run, otherwise the reason they are skipped.
	/// </summary>
	public static string? SkipReason {
		get {
			if (Environment.GetEnvironmentVariable(EnvironmentVariable) != "1") {
				return $"Live DocuSeal tests are opt-in. Set {EnvironmentVariable}=1 and configure the authorizationToken1/authorizationToken2/email1/email2 user secrets to run them.";
			}

			if (string.IsNullOrEmpty(Config.AuthorizationToken1)) {
				return $"{EnvironmentVariable} is set but the authorizationToken1 user secret is missing.";
			}

			return null;
		}
	}
}
