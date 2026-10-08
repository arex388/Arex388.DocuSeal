using Arex388.DocuSeal.Converters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal;

internal sealed class DocuSealClient(
	IServiceProvider services,
	HttpClient? httpClient = null) :
	IDocuSealClient {
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new() {
		Converters = {
			new CurrencyJsonConverter(),
			new EventTypeJsonConverter(),
			new FieldAlignJsonConverter(),
			new FieldFontJsonConverter(),
			new FieldFontTypeJsonConverter(),
			new FieldTypeJsonConverter(),
			new FieldVerticalAlignJsonConverter(),
			new PageSizeJsonConverter(),
			new SubmissionSourceJsonConverter(),
			new SubmissionStatusJsonConverter(),
			new SubmitterOrderJsonConverter(),
			new SubmitterStatusJsonConverter(),
			new TemplateSourceJsonConverter()
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly IValidator<ArchiveSubmission.Request> _archiveSubmissionRequestValidator = services.GetRequiredService<IValidator<ArchiveSubmission.Request>>();
	private readonly IValidator<ArchiveTemplate.Request> _archiveTemplateRequestValidator = services.GetRequiredService<IValidator<ArchiveTemplate.Request>>();
	private readonly IValidator<CloneTemplate.Request> _cloneTemplateRequestValidator = services.GetRequiredService<IValidator<CloneTemplate.Request>>();
	private readonly IValidator<CreateSubmission.Request> _createSubmissionRequestValidator = services.GetRequiredService<IValidator<CreateSubmission.Request>>();
	//private readonly IValidator<CreateSubmissionSimple.Request> _createSubmissionSimpleRequestValidator = services.GetRequiredService<IValidator<CreateSubmissionSimple.Request>>();
	private readonly IValidator<CreateTemplate.Request> _createTemplateRequestValidator = services.GetRequiredService<IValidator<CreateTemplate.Request>>();
	private readonly IValidator<CreateTemplateFromHtml.Request> _createTemplateFromHtmlRequestValidator = services.GetRequiredService<IValidator<CreateTemplateFromHtml.Request>>();
	private readonly IValidator<GetSubmission.Request> _getSubmissionRequestValidator = services.GetRequiredService<IValidator<GetSubmission.Request>>();
	private readonly IValidator<GetSubmitter.Request> _getSubmitterRequestValidator = services.GetRequiredService<IValidator<GetSubmitter.Request>>();
	private readonly IValidator<GetTemplate.Request> _getTemplateRequestValidator = services.GetRequiredService<IValidator<GetTemplate.Request>>();
	private readonly HttpClient _httpClient = httpClient
											  ?? services.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IDocuSealClient));
	private readonly IValidator<ListSubmissions.Request> _listSubmissionsRequestValidator = services.GetRequiredService<IValidator<ListSubmissions.Request>>();
	private readonly IValidator<ListSubmitters.Request> _listSubmittersRequestValidator = services.GetRequiredService<IValidator<ListSubmitters.Request>>();
	private readonly IValidator<ListTemplates.Request> _listTemplatesRequestValidator = services.GetRequiredService<IValidator<ListTemplates.Request>>();
	private readonly IValidator<MergeTemplates.Request> _mergeTemplateRequestValidator = services.GetRequiredService<IValidator<MergeTemplates.Request>>();
	private readonly IValidator<UpdateSubmitter.Request> _updateSubmitterRequestValidator = services.GetRequiredService<IValidator<UpdateSubmitter.Request>>();
	private readonly IValidator<UpdateTemplate.Request> _updateTemplateRequestValidator = services.GetRequiredService<IValidator<UpdateTemplate.Request>>();
	private readonly IValidator<UpdateTemplateDocuments.Request> _updateTemplateDocumentsRequestValidator = services.GetRequiredService<IValidator<UpdateTemplateDocuments.Request>>();

	public Guid Id { get; } = Guid.NewGuid();

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

		return await SendAsync<ArchiveSubmission.Response, ArchiveSubmission.Response>(HttpMethod.Delete, request.Endpoint, null, r => r.Error, r => r, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<ArchiveTemplate.Response, ArchiveTemplate.Response>(HttpMethod.Delete, request.Endpoint, null, r => r.Error, r => r, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Template, CloneTemplate.Response>(HttpMethod.Post, request.Endpoint, request, t => t.Error, t => new CloneTemplate.Response {
			Template = t
		}, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<CreatedSubmitters, CreateSubmission.Response>(HttpMethod.Post, request.Endpoint, request, DeserializeCreatedSubmitters, c => c.Error, c => new CreateSubmission.Response {
			SubmissionId = c.Submitters[0].SubmissionId,
			Submitters = c.Submitters
		}, cancellationToken).ConfigureAwait(false);
	}

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
	//			Errors = submission!.Error.HasValue()
	//				? [submission.Error]
	//				: []
	//		};
	//	} catch {
	//		return CreateSubmissionSimple.Response.Failed;
	//	}
	//}

	public Task<CreateTemplate.Response> CreateTemplateAsync(
		FileInfo file,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return Task.FromResult(CreateTemplate.Response.Cancelled);
		}

		var fileName = file.FullName;
		var endpoint = Path.GetExtension(fileName).ToLowerInvariant() switch {
			".docx" => CreateTemplate.Endpoints.Docx,
			".pdf" => CreateTemplate.Endpoints.Pdf,
			_ => null
		};

		if (endpoint is null) {
			return Task.FromResult(CreateTemplate.Response.Invalid(new ValidationResult([
				new ValidationFailure(nameof(file), "'File' must be a .pdf or .docx file.")
			])));
		}

		if (!file.Exists) {
			return Task.FromResult(CreateTemplate.Response.Invalid(new ValidationResult([
				new ValidationFailure(nameof(file), "'File' does not exist.")
			])));
		}

		byte[] fileBytes;

		try {
			fileBytes = File.ReadAllBytes(fileName);
		} catch {
			return Task.FromResult(CreateTemplate.Response.Failed);
		}

		return CreateTemplateAsync(new CreateTemplate.Request {
			Endpoint = endpoint,
			Documents = [
				new CreateTemplate.RequestDocument {
					Name = Path.GetFileNameWithoutExtension(fileName),
					FileBase64 = Convert.ToBase64String(fileBytes)
				}
			],
			Name = Path.GetFileNameWithoutExtension(fileName)
		}, cancellationToken);
	}

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

		return await SendAsync<Template, CreateTemplate.Response>(HttpMethod.Post, request.Endpoint, request, t => t.Error, t => new CreateTemplate.Response {
			Template = t
		}, cancellationToken).ConfigureAwait(false);
	}

	public async Task<CreateTemplateFromHtml.Response> CreateTemplateFromHtmlAsync(
		CreateTemplateFromHtml.Request request,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return CreateTemplateFromHtml.Response.Cancelled;
		}

		// ReSharper disable once MethodHasAsyncOverloadWithCancellation
		var validationResult = _createTemplateFromHtmlRequestValidator.Validate(request);

		if (!validationResult.IsValid) {
			return CreateTemplateFromHtml.Response.Invalid(validationResult);
		}

		return await SendAsync<Template, CreateTemplateFromHtml.Response>(HttpMethod.Post, request.Endpoint, request, t => t.Error, t => new CreateTemplateFromHtml.Response {
			Template = t
		}, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Submission, GetSubmission.Response>(HttpMethod.Get, request.Endpoint, null, s => s.Error, s => new GetSubmission.Response {
			Submission = s
		}, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Submitter, GetSubmitter.Response>(HttpMethod.Get, request.Endpoint, null, s => s.Error, s => new GetSubmitter.Response {
			Submitter = s
		}, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Template, GetTemplate.Response>(HttpMethod.Get, request.Endpoint, null, t => t.Error, t => new GetTemplate.Response {
			Template = t
		}, cancellationToken).ConfigureAwait(false);
	}

	public Task<ListSubmissions.Response> ListSubmissionsAsync(
		CancellationToken cancellationToken = default) => ListSubmissionsAsync(ListSubmissions.Request.Instance, cancellationToken);

	public async Task<ListSubmissions.Response> ListSubmissionsAsync(
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

		return await SendAsync<ListSubmissions.Response, ListSubmissions.Response>(HttpMethod.Get, request.Endpoint, null, r => r.Error, r => r, cancellationToken).ConfigureAwait(false);
	}

	public Task<ListSubmitters.Response> ListSubmittersAsync(
		CancellationToken cancellationToken = default) => ListSubmittersAsync(ListSubmitters.Request.Instance, cancellationToken);

	public async Task<ListSubmitters.Response> ListSubmittersAsync(
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

		return await SendAsync<ListSubmitters.Response, ListSubmitters.Response>(HttpMethod.Get, request.Endpoint, null, r => r.Error, r => r, cancellationToken).ConfigureAwait(false);
	}

	public Task<ListTemplates.Response> ListTemplatesAsync(
		CancellationToken cancellationToken = default) => ListTemplatesAsync(ListTemplates.Request.Instance, cancellationToken);

	public async Task<ListTemplates.Response> ListTemplatesAsync(
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

		return await SendAsync<ListTemplates.Response, ListTemplates.Response>(HttpMethod.Get, request.Endpoint, null, r => r.Error, r => r, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Template, MergeTemplates.Response>(HttpMethod.Post, request.Endpoint, request, t => t.Error, t => new MergeTemplates.Response {
			Template = t
		}, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Submitter, UpdateSubmitter.Response>(HttpMethod.Put, request.Endpoint, request, s => s.Error, s => new UpdateSubmitter.Response {
			Submitter = s
		}, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<UpdateTemplate.Response, UpdateTemplate.Response>(HttpMethod.Put, request.Endpoint, request, r => r.Error, r => r, cancellationToken).ConfigureAwait(false);
	}

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

		return await SendAsync<Template, UpdateTemplateDocuments.Response>(HttpMethod.Put, request.Endpoint, request, t => t.Error, t => new UpdateTemplateDocuments.Response {
			Template = t
		}, cancellationToken).ConfigureAwait(false);
	}

	//	============================================================================
	//	Utilities
	//	============================================================================

	/// <summary>
	/// The create-submission endpoint returns an array of submitters, one per
	/// submitter in the request, and all of them carry the same
	/// <c>submission_id</c>. An error arrives as a single object with an
	/// <c>error</c> member. Anything else is not a usable body and maps to
	/// <c>Failed</c>: an empty array, an array with a null element, an object
	/// without a non-empty <c>error</c> string, or a scalar.
	/// </summary>
	private static CreatedSubmitters? DeserializeCreatedSubmitters(
		string content) {
		using var document = JsonDocument.Parse(content);

		var root = document.RootElement;

		switch (root.ValueKind) {
			case JsonValueKind.Array:
				var submitters = root.Deserialize<IList<Submitter>>(_jsonSerializerOptions);

				return submitters is { Count: > 0 }
					   && submitters.All(s => s is not null)
					? new CreatedSubmitters(submitters, null)
					: null;
			case JsonValueKind.Object:
				if (!root.TryGetProperty("error", out var error)
					|| error.ValueKind != JsonValueKind.String) {
					return null;
				}

				var message = error.GetString();

				return message.HasValue()
					? new CreatedSubmitters([], message)
					: null;
			default:
				return null;
		}
	}

	private Task<TResponse> SendAsync<TPayload, TResponse>(
		HttpMethod method,
		string endpoint,
		object? body,
		Func<TPayload, string?> error,
		Func<TPayload, TResponse> success,
		CancellationToken cancellationToken)
		where TPayload : class
		where TResponse : ResponseBase<TResponse>, new() => SendAsync(method, endpoint, body, static content => JsonSerializer.Deserialize<TPayload>(content, _jsonSerializerOptions), error, success, cancellationToken);

	/// <summary>
	/// Sends one request and maps the body onto the no-throw response contract.
	/// The body is read regardless of status: an <c>error</c> member becomes
	/// <c>Errors = [error]</c> whether it arrives with a 2xx or a 4xx; a 2xx
	/// payload goes through <paramref name="success"/>; anything else (a non-2xx
	/// without an error member, an empty or malformed body, a transport
	/// exception) is <c>Failed</c>.
	/// </summary>
	private async Task<TResponse> SendAsync<TPayload, TResponse>(
		HttpMethod method,
		string endpoint,
		object? body,
		Func<string, TPayload?> deserialize,
		Func<TPayload, string?> error,
		Func<TPayload, TResponse> success,
		CancellationToken cancellationToken)
		where TPayload : class
		where TResponse : ResponseBase<TResponse>, new() {
		try {
			using var request = new HttpRequestMessage(method, endpoint);

			if (body is not null) {
				request.Content = JsonContent.Create(body, body.GetType(), options: _jsonSerializerOptions);
			}

			using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var payload = deserialize(content);

			if (payload is null) {
				return ResponseBase<TResponse>.Failed;
			}

			var message = error(payload);

			if (message.HasValue()) {
				return new TResponse {
					Errors = [message]
				};
			}

			return response.IsSuccessStatusCode
				? success(payload)
				: ResponseBase<TResponse>.Failed;
		} catch {
			return ResponseBase<TResponse>.Failed;
		}
	}

	//	============================================================================
	//	Types
	//	============================================================================

	/// <summary>
	/// A create-submission body once read: the submitters of an array body, or the
	/// <c>error</c> of an object body.
	/// </summary>
	private sealed record CreatedSubmitters(
		IList<Submitter> Submitters,
		string? Error);
}