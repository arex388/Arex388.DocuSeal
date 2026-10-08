using Arex388.DocuSeal.Converters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text;
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
			new TemplateSourceJsonConverter(),
			new WebhookEventTypeJsonConverter()
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	/// <summary>
	/// The serializer options every request and response goes through, shared with <see cref="DocuSealWebhook"/> so webhook payloads bind exactly as API responses do.
	/// </summary>
	internal static JsonSerializerOptions SerializerOptions => _jsonSerializerOptions;

	private static readonly UTF8Encoding _strictUtf8 = new(false, true);

	private readonly IValidator<ArchiveSubmission.Request> _archiveSubmissionRequestValidator = services.GetRequiredService<IValidator<ArchiveSubmission.Request>>();
	private readonly IValidator<ArchiveTemplate.Request> _archiveTemplateRequestValidator = services.GetRequiredService<IValidator<ArchiveTemplate.Request>>();
	private readonly IValidator<CloneTemplate.Request> _cloneTemplateRequestValidator = services.GetRequiredService<IValidator<CloneTemplate.Request>>();
	private readonly IValidator<CreateSubmission.Request> _createSubmissionRequestValidator = services.GetRequiredService<IValidator<CreateSubmission.Request>>();
	private readonly IValidator<CreateSubmissionFromDocx.Request> _createSubmissionFromDocxRequestValidator = services.GetRequiredService<IValidator<CreateSubmissionFromDocx.Request>>();
	private readonly IValidator<CreateSubmissionFromEmails.Request> _createSubmissionFromEmailsRequestValidator = services.GetRequiredService<IValidator<CreateSubmissionFromEmails.Request>>();
	private readonly IValidator<CreateSubmissionFromHtml.Request> _createSubmissionFromHtmlRequestValidator = services.GetRequiredService<IValidator<CreateSubmissionFromHtml.Request>>();
	private readonly IValidator<CreateSubmissionFromPdf.Request> _createSubmissionFromPdfRequestValidator = services.GetRequiredService<IValidator<CreateSubmissionFromPdf.Request>>();
	private readonly IValidator<CreateTemplate.Request> _createTemplateRequestValidator = services.GetRequiredService<IValidator<CreateTemplate.Request>>();
	private readonly IValidator<CreateTemplateFromHtml.Request> _createTemplateFromHtmlRequestValidator = services.GetRequiredService<IValidator<CreateTemplateFromHtml.Request>>();
	private readonly IValidator<GetSubmissionDocuments.Request> _getSubmissionDocumentsRequestValidator = services.GetRequiredService<IValidator<GetSubmissionDocuments.Request>>();
	private readonly IValidator<GetSubmission.Request> _getSubmissionRequestValidator = services.GetRequiredService<IValidator<GetSubmission.Request>>();
	private readonly IValidator<GetSubmitter.Request> _getSubmitterRequestValidator = services.GetRequiredService<IValidator<GetSubmitter.Request>>();
	private readonly IValidator<GetTemplate.Request> _getTemplateRequestValidator = services.GetRequiredService<IValidator<GetTemplate.Request>>();
	private readonly HttpClient _httpClient = httpClient
											  ?? services.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IDocuSealClient));
	private readonly IValidator<ListSubmissions.Request> _listSubmissionsRequestValidator = services.GetRequiredService<IValidator<ListSubmissions.Request>>();
	private readonly IValidator<ListSubmitters.Request> _listSubmittersRequestValidator = services.GetRequiredService<IValidator<ListSubmitters.Request>>();
	private readonly IValidator<ListTemplates.Request> _listTemplatesRequestValidator = services.GetRequiredService<IValidator<ListTemplates.Request>>();
	private readonly IValidator<MergeTemplates.Request> _mergeTemplateRequestValidator = services.GetRequiredService<IValidator<MergeTemplates.Request>>();
	private readonly IValidator<UpdateSubmission.Request> _updateSubmissionRequestValidator = services.GetRequiredService<IValidator<UpdateSubmission.Request>>();
	private readonly IValidator<UpdateSubmitter.Request> _updateSubmitterRequestValidator = services.GetRequiredService<IValidator<UpdateSubmitter.Request>>();
	private readonly IValidator<UpdateTemplate.Request> _updateTemplateRequestValidator = services.GetRequiredService<IValidator<UpdateTemplate.Request>>();
	private readonly IValidator<UpdateTemplateDocuments.Request> _updateTemplateDocumentsRequestValidator = services.GetRequiredService<IValidator<UpdateTemplateDocuments.Request>>();

	public Guid Id { get; } = Guid.NewGuid();

	public Task<ArchiveSubmission.Response> ArchiveSubmissionAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default) => ArchiveSubmissionAsync(new ArchiveSubmission.Request {
			Id = id
		}, cancellationToken);

	private Task<ArchiveSubmission.Response> ArchiveSubmissionAsync(
		ArchiveSubmission.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _archiveSubmissionRequestValidator, (r, ct) => SendAsync<ArchiveSubmission.Response, ArchiveSubmission.Response>(HttpMethod.Delete, r.Endpoint, null, p => p.Error, p => p, ct), cancellationToken);

	public Task<ArchiveTemplate.Response> ArchiveTemplateAsync(
		TemplateId id,
		CancellationToken cancellationToken = default) => ArchiveTemplateAsync(new ArchiveTemplate.Request {
			Id = id
		}, cancellationToken);

	private Task<ArchiveTemplate.Response> ArchiveTemplateAsync(
		ArchiveTemplate.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _archiveTemplateRequestValidator, (r, ct) => SendAsync<ArchiveTemplate.Response, ArchiveTemplate.Response>(HttpMethod.Delete, r.Endpoint, null, p => p.Error, p => p, ct), cancellationToken);

	public Task<CloneTemplate.Response> CloneTemplateAsync(
		CloneTemplate.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _cloneTemplateRequestValidator, (r, ct) => SendAsync<Template, CloneTemplate.Response>(HttpMethod.Post, r.Endpoint, r, t => t.Error, t => new CloneTemplate.Response {
			Template = t
		}, ct), cancellationToken);

	public Task<CreateSubmission.Response> CreateSubmissionAsync(
		CreateSubmission.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createSubmissionRequestValidator, (r, ct) => SendAsync<CreatedSubmitters, CreateSubmission.Response>(HttpMethod.Post, r.Endpoint, r, DeserializeCreatedSubmitters, c => c.Error, c => new CreateSubmission.Response {
			SubmissionId = c.Submitters[0].SubmissionId,
			Submitters = c.Submitters
		}, ct), cancellationToken);

	public Task<CreateSubmissionFromDocx.Response> CreateSubmissionFromDocxAsync(
		CreateSubmissionFromDocx.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createSubmissionFromDocxRequestValidator, (r, ct) => SendAsync<Submission, CreateSubmissionFromDocx.Response>(HttpMethod.Post, r.Endpoint, r, s => s.Error, s => new CreateSubmissionFromDocx.Response {
			Submission = s
		}, ct), cancellationToken);

	public Task<CreateSubmissionFromEmails.Response> CreateSubmissionFromEmailsAsync(
		CreateSubmissionFromEmails.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createSubmissionFromEmailsRequestValidator, (r, ct) => SendAsync<CreatedSubmitters, CreateSubmissionFromEmails.Response>(HttpMethod.Post, r.Endpoint, r, DeserializeCreatedSubmitters, c => c.Error, c => new CreateSubmissionFromEmails.Response {
			Submitters = c.Submitters
		}, ct), cancellationToken);

	public Task<CreateSubmissionFromHtml.Response> CreateSubmissionFromHtmlAsync(
		CreateSubmissionFromHtml.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createSubmissionFromHtmlRequestValidator, (r, ct) => SendAsync<Submission, CreateSubmissionFromHtml.Response>(HttpMethod.Post, r.Endpoint, r, s => s.Error, s => new CreateSubmissionFromHtml.Response {
			Submission = s
		}, ct), cancellationToken);

	public Task<CreateSubmissionFromPdf.Response> CreateSubmissionFromPdfAsync(
		CreateSubmissionFromPdf.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createSubmissionFromPdfRequestValidator, (r, ct) => SendAsync<Submission, CreateSubmissionFromPdf.Response>(HttpMethod.Post, r.Endpoint, r, s => s.Error, s => new CreateSubmissionFromPdf.Response {
			Submission = s
		}, ct), cancellationToken);

	public Task<CreateTemplate.Response> CreateTemplateAsync(
		FileInfo file,
		CancellationToken cancellationToken = default) {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return Task.FromResult(CreateTemplate.Response.Cancelled);
		}

		if (file is null) {
			return Task.FromResult(CreateTemplate.Response.Invalid(NullArgument("File")));
		}

		CreateTemplate.Request request;

		//	Reading the path, its extension and the file itself can all throw, so every
		//	step up to the request is inside the try and an exception is Failed.
		try {
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

			var fileBytes = File.ReadAllBytes(fileName);
			var name = Path.GetFileNameWithoutExtension(fileName);

			request = new CreateTemplate.Request {
				Endpoint = endpoint,
				Documents = [
					new CreateTemplate.RequestDocument {
						Name = name,
						FileBase64 = Convert.ToBase64String(fileBytes)
					}
				],
				Name = name
			};
		} catch {
			return Task.FromResult(CreateTemplate.Response.Failed);
		}

		return CreateTemplateAsync(request, cancellationToken);
	}

	public Task<CreateTemplate.Response> CreateTemplateAsync(
		CreateTemplate.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createTemplateRequestValidator, (r, ct) => SendAsync<Template, CreateTemplate.Response>(HttpMethod.Post, r.Endpoint, r, t => t.Error, t => new CreateTemplate.Response {
			Template = t
		}, ct), cancellationToken);

	public Task<CreateTemplateFromHtml.Response> CreateTemplateFromHtmlAsync(
		CreateTemplateFromHtml.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _createTemplateFromHtmlRequestValidator, (r, ct) => SendAsync<Template, CreateTemplateFromHtml.Response>(HttpMethod.Post, r.Endpoint, r, t => t.Error, t => new CreateTemplateFromHtml.Response {
			Template = t
		}, ct), cancellationToken);

	public Task<GetSubmission.Response> GetSubmissionAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default) => GetSubmissionAsync(new GetSubmission.Request {
			Id = id
		}, cancellationToken);

	private Task<GetSubmission.Response> GetSubmissionAsync(
		GetSubmission.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _getSubmissionRequestValidator, (r, ct) => SendAsync<Submission, GetSubmission.Response>(HttpMethod.Get, r.Endpoint, null, s => s.Error, s => new GetSubmission.Response {
			Submission = s
		}, ct), cancellationToken);

	public Task<GetSubmissionDocuments.Response> GetSubmissionDocumentsAsync(
		SubmissionId id,
		CancellationToken cancellationToken = default) => GetSubmissionDocumentsAsync(new GetSubmissionDocuments.Request {
			Id = id
		}, cancellationToken);

	public Task<GetSubmissionDocuments.Response> GetSubmissionDocumentsAsync(
		GetSubmissionDocuments.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _getSubmissionDocumentsRequestValidator, (r, ct) => SendAsync<GetSubmissionDocuments.Response, GetSubmissionDocuments.Response>(HttpMethod.Get, r.Endpoint, null, p => p.Error, p => p, ct), cancellationToken);

	public Task<GetSubmitter.Response> GetSubmitterAsync(
		SubmitterId id,
		CancellationToken cancellationToken = default) => GetSubmitterAsync(new GetSubmitter.Request {
			Id = id
		}, cancellationToken);

	private Task<GetSubmitter.Response> GetSubmitterAsync(
		GetSubmitter.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _getSubmitterRequestValidator, (r, ct) => SendAsync<Submitter, GetSubmitter.Response>(HttpMethod.Get, r.Endpoint, null, s => s.Error, s => new GetSubmitter.Response {
			Submitter = s
		}, ct), cancellationToken);

	public Task<GetTemplate.Response> GetTemplateAsync(
		TemplateId id,
		CancellationToken cancellationToken = default) => GetTemplateAsync(new GetTemplate.Request {
			Id = id
		}, cancellationToken);

	private Task<GetTemplate.Response> GetTemplateAsync(
		GetTemplate.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _getTemplateRequestValidator, (r, ct) => SendAsync<Template, GetTemplate.Response>(HttpMethod.Get, r.Endpoint, null, t => t.Error, t => new GetTemplate.Response {
			Template = t
		}, ct), cancellationToken);

	public Task<ListSubmissions.Response> ListSubmissionsAsync(
		CancellationToken cancellationToken = default) => ListSubmissionsAsync(ListSubmissions.Request.Instance, cancellationToken);

	public Task<ListSubmissions.Response> ListSubmissionsAsync(
		ListSubmissions.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _listSubmissionsRequestValidator, (r, ct) => SendAsync<ListSubmissions.Response, ListSubmissions.Response>(HttpMethod.Get, r.Endpoint, null, p => p.Error, p => p, ct), cancellationToken);

	public Task<ListSubmitters.Response> ListSubmittersAsync(
		CancellationToken cancellationToken = default) => ListSubmittersAsync(ListSubmitters.Request.Instance, cancellationToken);

	public Task<ListSubmitters.Response> ListSubmittersAsync(
		ListSubmitters.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _listSubmittersRequestValidator, (r, ct) => SendAsync<ListSubmitters.Response, ListSubmitters.Response>(HttpMethod.Get, r.Endpoint, null, p => p.Error, p => p, ct), cancellationToken);

	public Task<ListTemplates.Response> ListTemplatesAsync(
		CancellationToken cancellationToken = default) => ListTemplatesAsync(ListTemplates.Request.Instance, cancellationToken);

	public Task<ListTemplates.Response> ListTemplatesAsync(
		ListTemplates.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _listTemplatesRequestValidator, (r, ct) => SendAsync<ListTemplates.Response, ListTemplates.Response>(HttpMethod.Get, r.Endpoint, null, p => p.Error, p => p, ct), cancellationToken);

	public Task<MergeTemplates.Response> MergeTemplatesAsync(
		MergeTemplates.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _mergeTemplateRequestValidator, (r, ct) => SendAsync<Template, MergeTemplates.Response>(HttpMethod.Post, r.Endpoint, r, t => t.Error, t => new MergeTemplates.Response {
			Template = t
		}, ct), cancellationToken);

	public Task<UpdateSubmission.Response> UpdateSubmissionAsync(
		UpdateSubmission.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _updateSubmissionRequestValidator, (r, ct) => SendAsync<Submission, UpdateSubmission.Response>(HttpMethod.Put, r.Endpoint, r.Body, s => s.Error, s => new UpdateSubmission.Response {
			Submission = s
		}, ct), cancellationToken);

	public Task<UpdateSubmitter.Response> UpdateSubmitterAsync(
		UpdateSubmitter.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _updateSubmitterRequestValidator, (r, ct) => SendAsync<Submitter, UpdateSubmitter.Response>(HttpMethod.Put, r.Endpoint, r, s => s.Error, s => new UpdateSubmitter.Response {
			Submitter = s
		}, ct), cancellationToken);

	public Task<UpdateTemplate.Response> UpdateTemplateAsync(
		UpdateTemplate.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _updateTemplateRequestValidator, (r, ct) => SendAsync<UpdateTemplate.Response, UpdateTemplate.Response>(HttpMethod.Put, r.Endpoint, r, p => p.Error, p => p, ct), cancellationToken);

	public Task<UpdateTemplateDocuments.Response> UpdateTemplateDocumentsAsync(
		UpdateTemplateDocuments.Request request,
		CancellationToken cancellationToken = default) => GuardAsync(request, _updateTemplateDocumentsRequestValidator, (r, ct) => SendAsync<Template, UpdateTemplateDocuments.Response>(HttpMethod.Put, r.Endpoint, r, t => t.Error, t => new UpdateTemplateDocuments.Response {
			Template = t
		}, ct), cancellationToken);

	//	============================================================================
	//	Utilities
	//	============================================================================

	/// <summary>
	/// The one entry path every operation goes through, so the no-throw contract
	/// holds before the request reaches <c>SendAsync</c>: a cancelled token is
	/// <c>Cancelled</c>; a null request is <c>Invalid</c>; a failed validation is
	/// <c>Invalid</c>; and an exception thrown while validating, building the
	/// endpoint or body in <paramref name="send"/>, or sending is <c>Failed</c>.
	/// </summary>
	private static async Task<TResponse> GuardAsync<TRequest, TResponse>(
		TRequest? request,
		IValidator<TRequest> validator,
		Func<TRequest, CancellationToken, Task<TResponse>> send,
		CancellationToken cancellationToken)
		where TRequest : class
		where TResponse : ResponseBase<TResponse>, new() {
		if (cancellationToken.IsSupportedAndCancelled()) {
			return ResponseBase<TResponse>.Cancelled;
		}

		if (request is null) {
			return ResponseBase<TResponse>.Invalid(NullArgument("Request"));
		}

		try {
			// ReSharper disable once MethodHasAsyncOverloadWithCancellation
			var validationResult = validator.Validate(request);

			if (!validationResult.IsValid) {
				return ResponseBase<TResponse>.Invalid(validationResult);
			}

			return await send(request, cancellationToken).ConfigureAwait(false);
		} catch {
			return ResponseBase<TResponse>.Failed;
		}
	}

	private static ValidationResult NullArgument(
		string name) => new([
			new ValidationFailure(name, $"'{name}' must not be null.")
		]);

	/// <summary>
	/// The create-submission endpoints return an array of submitters: one per
	/// submitter in the request for <c>POST /submissions</c>, all carrying the same
	/// <c>submission_id</c>, and one per email address for <c>POST /submissions/emails</c>,
	/// each carrying its own. An error arrives as a single object with an
	/// <c>error</c> member. Anything else is not a usable body and maps to
	/// <c>Failed</c>: an empty array, an array with a null element, an object
	/// without a non-empty <c>error</c> string, or a scalar. The first token
	/// decides the shape, so an array is deserialized once, without a
	/// <see cref="JsonDocument"/>; in an object the last <c>error</c> member wins,
	/// as with <see cref="JsonElement.TryGetProperty(string, out JsonElement)"/>.
	/// </summary>
	private static CreatedSubmitters? DeserializeCreatedSubmitters(
		ReadOnlyMemory<byte> content) {
		var reader = new Utf8JsonReader(content.Span);

		reader.Read();

		switch (reader.TokenType) {
			case JsonTokenType.StartArray:
				var submitters = JsonSerializer.Deserialize<IList<Submitter>>(content.Span, _jsonSerializerOptions);

				return submitters is { Count: > 0 }
					   && submitters.All(s => s is not null)
					? new CreatedSubmitters(submitters, null)
					: null;
			case JsonTokenType.StartObject:
				//	Only the last error member's position is kept, and only that one is decoded after the scan, so an earlier
				//	occurrence that cannot be decoded does not hide the last one, as with JsonElement.TryGetProperty.
				var errorStart = -1L;

				while (reader.Read()
					   && reader.TokenType == JsonTokenType.PropertyName) {
					var isError = reader.ValueTextEquals("error"u8);

					reader.Read();

					if (isError) {
						errorStart = reader.TokenType == JsonTokenType.String
							? reader.TokenStartIndex
							: -1L;
					}

					reader.Skip();
				}

				//	The loop ends on the root's closing brace; anything after it but whitespace throws, which is Failed.
				reader.Read();

				if (errorStart < 0) {
					return null;
				}

				var errorReader = new Utf8JsonReader(content.Span.Slice((int)errorStart));

				errorReader.Read();

				var message = errorReader.GetString();

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
		where TResponse : ResponseBase<TResponse>, new() => SendAsync(method, endpoint, body, static content => JsonSerializer.Deserialize<TPayload>(content.Span, _jsonSerializerOptions), error, success, cancellationToken);

	/// <summary>
	/// Reads the whole response body as UTF-8 bytes for the serializer, without
	/// first decoding it to a string, when it is declared or detected as UTF-8 and
	/// is valid UTF-8: no charset or a <c>utf-8</c> charset, no UTF-16 or UTF-32
	/// byte order mark, and no invalid byte sequence. A UTF-8 byte order mark is
	/// skipped. For a valid UTF-8 body these bytes are exactly what decoding and
	/// re-encoding would produce, so the outcome is unchanged. Returns
	/// <see langword="null"/> when the body must be decoded through
	/// <see cref="ReadDecodedContentAsync"/> instead, which substitutes U+FFFD for
	/// invalid bytes as the client always has; validating up front matters because
	/// free-form members (<c>JsonObject</c>, <c>JsonElement</c>) bind raw bytes
	/// lazily and would otherwise fail only when a consumer reads them.
	/// </summary>
	private static async Task<ReadOnlyMemory<byte>?> ReadUtf8ContentAsync(
		HttpContent content) {
		var charSet = content.Headers.ContentType?.CharSet;

		if (charSet is not null
			&& !IsUtf8(charSet)) {
			return null;
		}

		var bytes = await content.ReadAsByteArrayAsync().ConfigureAwait(false);

		var start = bytes switch {
			[0xEF, 0xBB, 0xBF, ..] => 3,
			[0xFE, 0xFF, ..] or [0xFF, 0xFE, ..] or [0x00, 0x00, 0xFE, 0xFF, ..] => -1,
			_ => 0
		};

		if (start < 0) {
			return null;
		}

		//	GetCharCount validates without allocating; the strict encoding throws on the first invalid sequence.
		try {
			_strictUtf8.GetCharCount(bytes, start, bytes.Length - start);
		} catch (DecoderFallbackException) {
			return null;
		}

		return bytes.AsMemory(start);
	}

	/// <summary>
	/// Decodes the body exactly as the client always has, through
	/// <c>ReadAsStringAsync</c> (its charset, its byte order mark detection, and
	/// replacement characters for invalid bytes; it throws for a charset it does
	/// not know), and re-encodes it as UTF-8 for the serializer.
	/// </summary>
	private static async Task<ReadOnlyMemory<byte>> ReadDecodedContentAsync(
		HttpContent content) {
		var text = await content.ReadAsStringAsync().ConfigureAwait(false);

		return Encoding.UTF8.GetBytes(text);
	}

	//	Unquoted the way ReadAsStringAsync unquotes a charset: at most one pair of surrounding quotes.
	private static bool IsUtf8(
		string charSet) {
		if (charSet.Length > 2
			&& charSet[0] == '"'
			&& charSet[charSet.Length - 1] == '"') {
			charSet = charSet.Substring(1, charSet.Length - 2);
		}

		return string.Equals(charSet, "utf-8", StringComparison.OrdinalIgnoreCase);
	}

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
		Func<ReadOnlyMemory<byte>, TPayload?> deserialize,
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

			var utf8Content = await ReadUtf8ContentAsync(response.Content).ConfigureAwait(false);


			//	A valid UTF-8 body binds from its bytes; anything else is decoded as before, invalid bytes becoming U+FFFD.
			var payload = deserialize(utf8Content ?? await ReadDecodedContentAsync(response.Content).ConfigureAwait(false));

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
	/// A create-submission body (either endpoint) once read: the submitters of an array body, or the
	/// <c>error</c> of an object body.
	/// </summary>
	private sealed record CreatedSubmitters(
		IList<Submitter> Submitters,
		string? Error);
}