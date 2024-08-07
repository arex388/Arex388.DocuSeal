namespace Arex388.DocuSeal;

/// <summary>
/// DocuSeal.co API client.
/// </summary>
public interface IDocuSealClient {
	/// <summary>
	/// Archive a submission.
	/// </summary>
	/// <param name="id">The submission's id.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed.</returns>
	Task<ArchiveSubmission.Response> ArchiveSubmissionAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Archive a template.
	/// </summary>
	/// <param name="id">The template's id.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed.</returns>
	Task<ArchiveTemplate.Response> ArchiveTemplateAsync(
		TemplateId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Clone a template.
	/// </summary>
	/// <param name="request">The template cloning request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the cloned template.</returns>
	Task<CloneTemplate.Response> CloneTemplateAsync(
		CloneTemplate.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a submission.
	/// </summary>
	/// <param name="request">The submission creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created submission.</returns>
	Task<CreateSubmission.Response> CreateSubmissionAsync(
		CreateSubmission.Request request,
		CancellationToken cancellationToken = default);

	//Task<CreateSubmissionSimple.Response> CreateSubmissionSimpleAsync(
	//	CreateSubmissionSimple.Request request,
	//	CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a template.
	/// </summary>
	/// <param name="file">The file to use for creating the template.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created template.</returns>
	Task<CreateTemplate.Response> CreateTemplateAsync(
		FileInfo file,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a template.
	/// </summary>
	/// <param name="request">The template creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created template.</returns>
	Task<CreateTemplate.Response> CreateTemplateAsync(
		CreateTemplate.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Get a submission.
	/// </summary>
	/// <param name="id">The submission's id.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submission.</returns>
	Task<GetSubmission.Response> GetSubmissionAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Get a submitter.
	/// </summary>
	/// <param name="id">The submitter's id.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submitter.</returns>
	Task<GetSubmitter.Response> GetSubmitterAsync(
		SubmitterId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Get a template.
	/// </summary>
	/// <param name="id">The template's id.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the template.</returns>
	Task<GetTemplate.Response> GetTemplateAsync(
		TemplateId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submissions.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submissions.</returns>
	[NotNull]
	Task<ListSubmissions.Response?> ListSubmissionsAsync(
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submissions.
	/// </summary>
	/// <param name="request">The list submissions request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submissions.</returns>
	[NotNull]
	Task<ListSubmissions.Response?> ListSubmissionsAsync(
		ListSubmissions.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submitters.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submitters.</returns>
	[NotNull]
	Task<ListSubmitters.Response?> ListSubmittersAsync(
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submitters.
	/// </summary>
	/// <param name="request">The list submitters request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submitters.</returns>
	[NotNull]
	Task<ListSubmitters.Response?> ListSubmittersAsync(
		ListSubmitters.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List templates.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the templates.</returns>
	[NotNull]
	Task<ListTemplates.Response?> ListTemplatesAsync(
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List templates.
	/// </summary>
	/// <param name="request">The list templates request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the templates.</returns>
	[NotNull]
	Task<ListTemplates.Response?> ListTemplatesAsync(
		ListTemplates.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Merge templates.
	/// </summary>
	/// <param name="request">The merge templates request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the merged template.</returns>
	Task<MergeTemplates.Response> MergeTemplatesAsync(
		MergeTemplates.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Update a submitter.
	/// </summary>
	/// <param name="request">The update submitter request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed.</returns>
	Task<UpdateSubmitter.Response> UpdateSubmitterAsync(
		UpdateSubmitter.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Update a template.
	/// </summary>
	/// <param name="request">The update template request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed.</returns>
	Task<UpdateTemplate.Response> UpdateTemplateAsync(
		UpdateTemplate.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Update a template's documents.
	/// </summary>
	/// <param name="request">The update template documents request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed.</returns>
	Task<UpdateTemplateDocuments.Response> UpdateTemplateDocumentsAsync(
		UpdateTemplateDocuments.Request request,
		CancellationToken cancellationToken = default);
}