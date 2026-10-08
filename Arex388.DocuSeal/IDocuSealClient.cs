namespace Arex388.DocuSeal;

/// <summary>
/// DocuSeal API client.
/// </summary>
/// <remarks>
/// Members do not throw. A cancelled token, a <see langword="null" /> request, a validation failure, an API error, and an exception while building or sending the request are all reported through <see cref="ResponseBase{TResponse}.Errors" /> with <see cref="ResponseBase{TResponse}.Success" /> set to <see langword="false" />.
/// </remarks>
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

	/// <summary>
	/// Create a one-off submission from DOCX documents with dynamic content variables, without a saved template.
	/// </summary>
	/// <param name="request">The submission creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created submission.</returns>
	Task<CreateSubmissionFromDocx.Response> CreateSubmissionFromDocxAsync(
		CreateSubmissionFromDocx.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a submission for each of the given email addresses from a template, which sends each of them a signature request unless <see cref="CreateSubmissionFromEmails.Request.MustEmail" /> is <see langword="false" />.
	/// </summary>
	/// <param name="request">The submissions creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created submissions' submitters.</returns>
	Task<CreateSubmissionFromEmails.Response> CreateSubmissionFromEmailsAsync(
		CreateSubmissionFromEmails.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a one-off submission from HTML documents with field tags, without a saved template.
	/// </summary>
	/// <param name="request">The submission creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created submission.</returns>
	Task<CreateSubmissionFromHtml.Response> CreateSubmissionFromHtmlAsync(
		CreateSubmissionFromHtml.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a one-off submission from PDF documents, without a saved template.
	/// </summary>
	/// <param name="request">The submission creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created submission.</returns>
	Task<CreateSubmissionFromPdf.Response> CreateSubmissionFromPdfAsync(
		CreateSubmissionFromPdf.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Create a template.
	/// </summary>
	/// <param name="file">The file to use for creating the template. Must be a <c>.pdf</c> or <c>.docx</c> file; a <see langword="null" /> file, any other extension, or a file that does not exist returns an invalid response without calling the API, and a file that cannot be read returns a failed response.</param>
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
	/// Create a template from HTML with field tags. Set <see cref="CreateTemplateFromHtml.Request.Html" /> for a single document or <see cref="CreateTemplateFromHtml.Request.Documents" /> for several; one of the two is required.
	/// </summary>
	/// <param name="request">The template creation request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the created template.</returns>
	Task<CreateTemplateFromHtml.Response> CreateTemplateFromHtmlAsync(
		CreateTemplateFromHtml.Request request,
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
	/// Get the documents of a submission. If the submission has been completed, the final signed documents are returned; otherwise the partially filled documents.
	/// </summary>
	/// <param name="id">The submission's id.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submission's documents.</returns>
	Task<GetSubmissionDocuments.Response> GetSubmissionDocumentsAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Get the documents of a submission, optionally merged into a single PDF. If the submission has been completed, the final signed documents are returned; otherwise the partially filled documents.
	/// </summary>
	/// <param name="request">The get submission documents request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submission's documents.</returns>
	Task<GetSubmissionDocuments.Response> GetSubmissionDocumentsAsync(
		GetSubmissionDocuments.Request request,
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
	Task<ListSubmissions.Response> ListSubmissionsAsync(
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submissions.
	/// </summary>
	/// <param name="request">The list submissions request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submissions.</returns>
	Task<ListSubmissions.Response> ListSubmissionsAsync(
		ListSubmissions.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submitters.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submitters.</returns>
	Task<ListSubmitters.Response> ListSubmittersAsync(
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List submitters.
	/// </summary>
	/// <param name="request">The list submitters request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the submitters.</returns>
	Task<ListSubmitters.Response> ListSubmittersAsync(
		ListSubmitters.Request request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List templates.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the templates.</returns>
	Task<ListTemplates.Response> ListTemplatesAsync(
		CancellationToken cancellationToken = default);

	/// <summary>
	/// List templates.
	/// </summary>
	/// <param name="request">The list templates request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the templates.</returns>
	Task<ListTemplates.Response> ListTemplatesAsync(
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
	/// Update a submission: change its name or expiration, or archive or unarchive it. Set <see cref="UpdateSubmission.Request.IsArchived" /> to <see langword="false" /> to unarchive; set <see cref="UpdateSubmission.Request.ClearExpiration" /> to remove the expiration.
	/// </summary>
	/// <param name="request">The update submission request.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A response indicating if the operation completed with the updated submission.</returns>
	Task<UpdateSubmission.Response> UpdateSubmissionAsync(
		UpdateSubmission.Request request,
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