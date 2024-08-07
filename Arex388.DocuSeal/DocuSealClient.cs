using Arex388.DocuSeal.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace Arex388.DocuSeal;

internal sealed class DocuSealClient :
	IDocuSealClient {
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new() {
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly IValidator<ArchiveSubmission.Request> _archiveSubmissionRequestValidator;
	private readonly IValidator<ArchiveTemplate.Request> _archiveTemplateRequestValidator;
	private readonly IValidator<CloneTemplate.Request> _cloneTemplateRequestValidator;
	private readonly IValidator<CreateSubmission.Request> _createSubmissionRequestValidator;
	//private readonly IValidator<CreateSubmissionSimple.Request> _createSubmissionSimpleRequestValidator;
	private readonly IValidator<CreateTemplate.Request> _createTemplateRequestValidator;
	private readonly IValidator<GetSubmission.Request> _getSubmissionRequestValidator;
	private readonly IValidator<GetSubmitter.Request> _getSubmitterRequestValidator;
	private readonly IValidator<GetTemplate.Request> _getTemplateRequestValidator;
	private readonly HttpClient _httpClient;
	private readonly IValidator<ListSubmissions.Request> _listSubmissionsRequestValidator;
	private readonly IValidator<ListSubmitters.Request> _listSubmittersRequestValidator;
	private readonly IValidator<ListTemplates.Request> _listTemplatesRequestValidator;
	private readonly IValidator<MergeTemplates.Request> _mergeTemplateRequestValidator;
	private readonly IServiceProvider _services;
	private readonly IValidator<UpdateSubmitter.Request> _updateSubmitterRequestValidator;
	private readonly IValidator<UpdateTemplate.Request> _updateTemplateRequestValidator;
	private readonly IValidator<UpdateTemplateDocuments.Request> _updateTemplateDocumentsRequestValidator;

	public DocuSealClient(
		IServiceProvider services,
		HttpClient? httpClient = null) {
		var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();

		_archiveSubmissionRequestValidator = services.GetRequiredService<IValidator<ArchiveSubmission.Request>>();
		_archiveTemplateRequestValidator = services.GetRequiredService<IValidator<ArchiveTemplate.Request>>();
		_cloneTemplateRequestValidator = services.GetRequiredService<IValidator<CloneTemplate.Request>>();
		_createSubmissionRequestValidator = services.GetRequiredService<IValidator<CreateSubmission.Request>>();
		//_createSubmissionSimpleRequestValidator = services.GetRequiredService<IValidator<CreateSubmissionSimple.Request>>();
		_createTemplateRequestValidator = services.GetRequiredService<IValidator<CreateTemplate.Request>>();
		_httpClient = httpClient
					  ?? httpClientFactory.CreateClient(nameof(IDocuSealClient));
		_getSubmissionRequestValidator = services.GetRequiredService<IValidator<GetSubmission.Request>>();
		_getSubmitterRequestValidator = services.GetRequiredService<IValidator<GetSubmitter.Request>>();
		_getTemplateRequestValidator = services.GetRequiredService<IValidator<GetTemplate.Request>>();
		_listSubmissionsRequestValidator = services.GetRequiredService<IValidator<ListSubmissions.Request>>();
		_listSubmittersRequestValidator = services.GetRequiredService<IValidator<ListSubmitters.Request>>();
		_listTemplatesRequestValidator = services.GetRequiredService<IValidator<ListTemplates.Request>>();
		_mergeTemplateRequestValidator = services.GetRequiredService<IValidator<MergeTemplates.Request>>();
		_services = services;
		_updateSubmitterRequestValidator = services.GetRequiredService<IValidator<UpdateSubmitter.Request>>();
		_updateTemplateRequestValidator = services.GetRequiredService<IValidator<UpdateTemplate.Request>>();
		_updateTemplateDocumentsRequestValidator = services.GetRequiredService<IValidator<UpdateTemplateDocuments.Request>>();
	}

	//	============================================================================
	//	Actions
	//	============================================================================

	/// <inheritdoc />
	public Task<ArchiveSubmission.Response> ArchiveSubmissionAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default) => ArchiveSubmissionAsync(new ArchiveSubmission.Request {
			Id = id
		}, cancellationToken);

	private async Task<ArchiveSubmission.Response> ArchiveSubmissionAsync(
		ArchiveSubmission.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return ArchiveSubmission.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _archiveSubmissionRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return ArchiveSubmission.Response.Invalid(validationResult);
		}

		try {
			var submission = await _httpClient.DeleteFromJsonAsync<Submission>(request.Endpoint, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new ArchiveSubmission.Response {
				Errors = submission.HasErred()
					? [submission!.Error!]
					: []
			};
		} catch {
			return ArchiveSubmission.Response.Failed;
		}
	}

	/// <inheritdoc />
	public Task<ArchiveTemplate.Response> ArchiveTemplateAsync(
		TemplateId id,
		CancellationToken cancellationToken = default) => ArchiveTemplateAsync(new ArchiveTemplate.Request {
			Id = id
		}, cancellationToken);

	private async Task<ArchiveTemplate.Response> ArchiveTemplateAsync(
		ArchiveTemplate.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return ArchiveTemplate.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _archiveTemplateRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return ArchiveTemplate.Response.Invalid(validationResult);
		}

		try {
			var template = await _httpClient.DeleteFromJsonAsync<Template>(request.Endpoint, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new ArchiveTemplate.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: []
			};
		} catch {
			return ArchiveTemplate.Response.Failed;
		}
	}

	/// <inheritdoc />
	public async Task<CloneTemplate.Response> CloneTemplateAsync(
		CloneTemplate.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return CloneTemplate.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _cloneTemplateRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return CloneTemplate.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PostAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var template = await response.Content.ReadFromJsonAsync<Template>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new CloneTemplate.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: [],
				Template = template.HasErred()
					? null
					: template
			};
		} catch {
			return CloneTemplate.Response.Failed;
		}
	}

	/// <inheritdoc />
	public async Task<CreateSubmission.Response> CreateSubmissionAsync(
		CreateSubmission.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return CreateSubmission.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _createSubmissionRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return CreateSubmission.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PostAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			Submission? submission;

			try {
				var submissions = JsonSerializer.Deserialize<IList<Submission>>(responseContent, _jsonSerializerOptions);

				submission = submissions![0];
			} catch {
				submission = JsonSerializer.Deserialize<Submission>(responseContent, _jsonSerializerOptions);
			}

			return new CreateSubmission.Response {
				Errors = submission.HasErred()
					? [submission!.Error!]
					: [],
				Submission = submission.HasErred()
					? null
					: submission
			};
		} catch {
			return CreateSubmission.Response.Failed;
		}
	}

	///// <inheritdoc />
	//public async Task<CreateSubmissionSimple.Response> CreateSubmissionSimpleAsync(
	//	CreateSubmissionSimple.Request request,
	//	CancellationToken cancellationToken = default) {
	//	if (cancellationToken.IsSupportedAndCancelled()) {
	//		return CreateSubmissionSimple.Response.Cancelled;
	//	}

	//	// ReSharper disable once MethodHasAsyncOverloadWithCancellation
	//	var validationResult = _createSubmissionSimpleRequestValidator.Validate(request);

	//	if (!validationResult.IsValid) {
	//		return CreateSubmissionSimple.Response.Invalid(validationResult);
	//	}

	//	try {
	//		var response = await _httpClient.PostAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
	//		var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
	//		Submission? submission;

	//		try {
	//			var submissions = JsonSerializer.Deserialize<IList<Submission>>(responseContent, _jsonSerializerOptions);

	//			submission = submissions![0];
	//		} catch {
	//			submission = JsonSerializer.Deserialize<Submission>(responseContent, _jsonSerializerOptions);
	//		}

	//		return new CreateSubmissionSimple.Response {
	//			Errors = submission.HasErred()
	//				? [submission!.Error!]
	//				: []
	//		};
	//	} catch {
	//		return CreateSubmissionSimple.Response.Failed;
	//	}
	//}

	/// <inheritdoc />
	public Task<CreateTemplate.Response> CreateTemplateAsync(
		FileInfo file,
		CancellationToken cancellationToken = default) {
		if (!file.Exists
			|| cancellationToken.IsSupportedAndCancelled()) {
			return Task.FromResult(CreateTemplate.Response.Cancelled);
		}

		var fileName = file.FullName;
		var fileBytes = File.ReadAllBytes(fileName);
		var fileExtension = Path.GetExtension(fileName);

		return CreateTemplateAsync(new CreateTemplate.Request {
			Endpoint = fileExtension switch {
				".docx" => CreateTemplate.Endpoints.Docx,
				".pdf" => CreateTemplate.Endpoints.Pdf,
				_ => null!
			},
			Documents = [
				new CreateTemplate.RequestDocument {
					Name = Path.GetFileNameWithoutExtension(fileName),
					FileBase64 = Convert.ToBase64String(fileBytes)
				}
			],
			Name = Path.GetFileNameWithoutExtension(fileName)
		}, cancellationToken);
	}

	/// <inheritdoc />
	public async Task<CreateTemplate.Response> CreateTemplateAsync(
		CreateTemplate.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return CreateTemplate.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _createTemplateRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return CreateTemplate.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PostAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var template = await response.Content.ReadFromJsonAsync<Template>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new CreateTemplate.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: [],
				Template = template.HasErred()
					? null
					: template
			};
		} catch {
			return CreateTemplate.Response.Failed;
		}
	}

	/// <inheritdoc />
	public Task<GetSubmission.Response> GetSubmissionAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default) => GetSubmissionAsync(new GetSubmission.Request {
			Id = id
		}, cancellationToken);

	private async Task<GetSubmission.Response> GetSubmissionAsync(
		GetSubmission.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return GetSubmission.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _getSubmissionRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return GetSubmission.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.GetAsync(request.Endpoint, cancellationToken).ConfigureAwait(false);
			var submission = await response.Content.ReadFromJsonAsync<Submission>(cancellationToken).ConfigureAwait(false);

			return new GetSubmission.Response {
				Errors = submission.HasErred()
					? [submission!.Error!]
					: [],
				Submission = submission.HasErred()
					? null
					: submission
			};
		} catch {
			return GetSubmission.Response.Failed;
		}
	}

	/// <inheritdoc />
	public Task<GetSubmitter.Response> GetSubmitterAsync(
		SubmitterId id,
		CancellationToken cancellationToken = default) => GetSubmitterAsync(new GetSubmitter.Request {
			Id = id
		}, cancellationToken);

	private async Task<GetSubmitter.Response> GetSubmitterAsync(
		GetSubmitter.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return GetSubmitter.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _getSubmitterRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return GetSubmitter.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.GetAsync(request.Endpoint, cancellationToken).ConfigureAwait(false);
			var submitter = await response.Content.ReadFromJsonAsync<Submitter>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new GetSubmitter.Response {
				Errors = submitter.HasErred()
					? [submitter!.Error!]
					: [],
				Submitter = submitter.HasErred()
					? null
					: submitter
			};
		} catch {
			return GetSubmitter.Response.Failed;
		}
	}

	/// <inheritdoc />
	public Task<GetTemplate.Response> GetTemplateAsync(
		TemplateId id,
		CancellationToken cancellationToken = default) => GetTemplateAsync(new GetTemplate.Request {
			Id = id
		}, cancellationToken);

	private async Task<GetTemplate.Response> GetTemplateAsync(
		GetTemplate.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return GetTemplate.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _getTemplateRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return GetTemplate.Response.Invalid(validationResult);
		}

		try {
			var template = await _httpClient.GetFromJsonAsync<Template>(request.Endpoint, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new GetTemplate.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: [],
				Template = template.HasErred()
					? null
					: template
			};
		} catch {
			return GetTemplate.Response.Failed;
		}
	}

	/// <inheritdoc />
	[NotNull]
	public Task<ListSubmissions.Response?> ListSubmissionsAsync(
		CancellationToken cancellationToken = default) => ListSubmissionsAsync(ListSubmissions.Request.Instance, cancellationToken);

	/// <inheritdoc />
	[NotNull]
	public async Task<ListSubmissions.Response?> ListSubmissionsAsync(
		ListSubmissions.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return ListSubmissions.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _listSubmissionsRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return ListSubmissions.Response.Invalid(validationResult);
		}

		try {
			return await _httpClient.GetFromJsonAsync<ListSubmissions.Response?>(request.Endpoint, cancellationToken).ConfigureAwait(false);
		} catch {
			return ListSubmissions.Response.Failed;
		}
	}

	/// <inheritdoc />
	[NotNull]
	public Task<ListSubmitters.Response?> ListSubmittersAsync(
		CancellationToken cancellationToken = default) => ListSubmittersAsync(ListSubmitters.Request.Instance, cancellationToken);

	/// <inheritdoc />
	[NotNull]
	public async Task<ListSubmitters.Response?> ListSubmittersAsync(
		ListSubmitters.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return ListSubmitters.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _listSubmittersRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return ListSubmitters.Response.Invalid(validationResult);
		}

		try {
			return await _httpClient.GetFromJsonAsync<ListSubmitters.Response?>(request.Endpoint, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
		} catch {
			return ListSubmitters.Response.Failed;
		}
	}

	/// <inheritdoc />
	[NotNull]
	public Task<ListTemplates.Response?> ListTemplatesAsync(
		CancellationToken cancellationToken = default) => ListTemplatesAsync(ListTemplates.Request.Instance, cancellationToken);

	/// <inheritdoc />
	[NotNull]
	public async Task<ListTemplates.Response?> ListTemplatesAsync(
		ListTemplates.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return ListTemplates.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _listTemplatesRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return ListTemplates.Response.Invalid(validationResult);
		}

		try {
			return await _httpClient.GetFromJsonAsync<ListTemplates.Response?>(request.Endpoint, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
		} catch {
			return ListTemplates.Response.Failed;
		}
	}

	/// <inheritdoc />
	public async Task<MergeTemplates.Response> MergeTemplatesAsync(
		MergeTemplates.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return MergeTemplates.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _mergeTemplateRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return MergeTemplates.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PostAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var template = await response.Content.ReadFromJsonAsync<Template>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new MergeTemplates.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: [],
				Template = template.HasErred()
					? null
					: template
			};
		} catch {
			return MergeTemplates.Response.Failed;
		}
	}

	/// <inheritdoc />
	public async Task<UpdateSubmitter.Response> UpdateSubmitterAsync(
		UpdateSubmitter.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return UpdateSubmitter.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _updateSubmitterRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return UpdateSubmitter.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PutAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var submitter = await response.Content.ReadFromJsonAsync<Submitter>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new UpdateSubmitter.Response {
				Errors = submitter.HasErred()
					? [submitter!.Error!]
					: []
			};
		} catch {
			return UpdateSubmitter.Response.Failed;
		}
	}

	/// <inheritdoc />
	public async Task<UpdateTemplate.Response> UpdateTemplateAsync(
		UpdateTemplate.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return UpdateTemplate.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _updateTemplateRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return UpdateTemplate.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PutAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var template = await response.Content.ReadFromJsonAsync<Template>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new UpdateTemplate.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: []
			};
		} catch {
			return UpdateTemplate.Response.Failed;
		}
	}

	/// <inheritdoc />
	public async Task<UpdateTemplateDocuments.Response> UpdateTemplateDocumentsAsync(
		UpdateTemplateDocuments.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return UpdateTemplateDocuments.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _updateTemplateDocumentsRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return UpdateTemplateDocuments.Response.Invalid(validationResult);
		}

		try {
			var response = await _httpClient.PutAsJsonAsync(request.Endpoint, request, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
			var template = await response.Content.ReadFromJsonAsync<Template>(_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

			return new UpdateTemplateDocuments.Response {
				Errors = template.HasErred()
					? [template!.Error!]
					: []
			};
		} catch {
			return UpdateTemplateDocuments.Response.Failed;
		}
	}
}