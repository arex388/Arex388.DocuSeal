using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Arex388.DocuSeal.Converters;

/// <summary>
/// Normalises <see cref="Submission.Id"/> to the submission's id on every path.
/// The GET and list bodies carry the submission's id as <c>id</c>; the
/// create-submission body is submitter-shaped, so its <c>id</c> is the submitter
/// and <c>submission_id</c> is the submission. When <c>submission_id</c> is
/// present it replaces <c>id</c> before the object binds.
/// </summary>
internal sealed class SubmissionJsonConverter :
	JsonConverter<Submission> {
	private volatile InnerOptions? _innerOptions;

	public override Submission? Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options) {
		if (JsonNode.Parse(ref reader) is not JsonObject submission) {
			throw new JsonException($"Expected a JSON object for {nameof(Submission)}.");
		}

		//	A JSON null submission_id is dropped without touching id, so a null
		//	never replaces a usable id.
		if (submission.TryGetPropertyValue("submission_id", out var submissionId)) {
			submission.Remove("submission_id");

			if (submissionId is not null) {
				submission["id"] = submissionId;
			}
		}

		return submission.Deserialize<Submission>(GetInnerOptions(options));
	}

	public override void Write(
		Utf8JsonWriter writer,
		Submission value,
		JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value, GetInnerOptions(options));

	//	============================================================================
	//	Utilities
	//	============================================================================

	/// <summary>
	/// The caller's options without this converter, so the inner (de)serialization
	/// binds <see cref="Submission"/> by its attributes instead of recursing. The
	/// last (options, copy) pair is cached and rebuilt when a call arrives with a
	/// different options reference. Lock-free: racing calls may each build an
	/// equivalent copy, and the last write wins.
	/// </summary>
	private JsonSerializerOptions GetInnerOptions(
		JsonSerializerOptions options) {
		var cached = _innerOptions;

		if (cached is not null
			&& ReferenceEquals(cached.Source, options)) {
			return cached.Copy;
		}

		var copy = new JsonSerializerOptions(options);

		for (var i = copy.Converters.Count - 1; i >= 0; i--) {
			if (copy.Converters[i] is SubmissionJsonConverter) {
				copy.Converters.RemoveAt(i);
			}
		}

		_innerOptions = new InnerOptions(options, copy);

		return copy;
	}

	private sealed class InnerOptions(
		JsonSerializerOptions source,
		JsonSerializerOptions copy) {
		public JsonSerializerOptions Source { get; } = source;
		public JsonSerializerOptions Copy { get; } = copy;
	}
}