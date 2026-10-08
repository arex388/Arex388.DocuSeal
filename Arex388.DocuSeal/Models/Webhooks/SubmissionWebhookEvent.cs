namespace Arex388.DocuSeal;

/// <summary>
/// A submission webhook event: <see cref="WebhookEventType.SubmissionCreated"/>, <see cref="WebhookEventType.SubmissionCompleted"/>, or <see cref="WebhookEventType.SubmissionExpired"/>.
/// </summary>
public sealed class SubmissionWebhookEvent :
	WebhookEvent {
	/// <summary>
	/// The submission webhook event's data. It is the submission, in the same shape <see cref="IDocuSealClient.GetSubmissionAsync(SubmissionId, CancellationToken)"/> returns.
	/// </summary>
	public Submission Data { get; init; } = null!;
}