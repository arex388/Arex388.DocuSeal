namespace Arex388.DocuSeal;

/// <summary>
/// A webhook event type. The webhook tokens (<c>form.viewed</c>) are disjoint from the submission event tokens <see cref="EventType"/> reads (<c>view_form</c>).
/// </summary>
public enum WebhookEventType {
	/// <summary>
	/// An unknown webhook event.
	/// </summary>
	Unknown,

	/// <summary>
	/// The submitter first opened the form (<c>form.viewed</c>).
	/// </summary>
	FormViewed,

	/// <summary>
	/// The submitter started filling out the form (<c>form.started</c>).
	/// </summary>
	FormStarted,

	/// <summary>
	/// The submitter completed and signed the form (<c>form.completed</c>).
	/// </summary>
	FormCompleted,

	/// <summary>
	/// The submitter declined the form (<c>form.declined</c>).
	/// </summary>
	FormDeclined,

	/// <summary>
	/// The submission was created (<c>submission.created</c>).
	/// </summary>
	SubmissionCreated,

	/// <summary>
	/// The submission was completed by every signing party (<c>submission.completed</c>).
	/// </summary>
	SubmissionCompleted,

	/// <summary>
	/// The submission expired (<c>submission.expired</c>).
	/// </summary>
	SubmissionExpired,

	/// <summary>
	/// The submission was archived (<c>submission.archived</c>).
	/// </summary>
	SubmissionArchived,

	/// <summary>
	/// The template was created (<c>template.created</c>).
	/// </summary>
	TemplateCreated,

	/// <summary>
	/// The template was updated (<c>template.updated</c>).
	/// </summary>
	TemplateUpdated,

	/// <summary>
	/// The template was archived (<c>template.archived</c>).
	/// </summary>
	TemplateArchived
}