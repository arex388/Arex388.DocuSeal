using FluentValidation;
using System.Globalization;
using System.Text.Json.Serialization;
using static Arex388.DocuSeal.UpdateSubmission;

namespace Arex388.DocuSeal;

/// <summary>
/// The API endpoint allows you to update a submission: change its name or expiration date, and archive or unarchive it.
/// </summary>
public static class UpdateSubmission {
	/// <summary>
	/// Update submission request. Only the members that are set are sent; the others are left unchanged.
	/// </summary>
	public sealed class Request {
		//	Serialized when ClearExpiration is false. When it is true the client sends RequestBody instead, so the explicit null never needs the global ignore condition weakened.
		internal object Body => ClearExpiration
			? new RequestBody {
				IsArchived = IsArchived,
				Name = Name
			}
			: this;

		/// <summary>
		/// Set <see langword="true" /> to remove the submission's expiration, which sends <c>"expire_at": null</c>. Cannot be combined with <see cref="ExpireAtUtc" />.
		/// </summary>
		[JsonIgnore]
		public bool ClearExpiration { get; init; }

		internal string Endpoint => $"submissions/{Id}";

		//	The API takes expire_at as a string such as "2024-09-01 12:00:00 UTC", not as an ISO 8601 date-time.
		[JsonInclude, JsonPropertyName("expire_at")]
		internal string? ExpireAt => ExpireAtUtc?.AsUtc().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

		/// <summary>
		/// The date and time, in UTC, after which the submission becomes unavailable for signature. A value of <see cref="DateTimeKind.Local" /> kind is converted to UTC; a value of <see cref="DateTimeKind.Unspecified" /> kind is taken to already be UTC. Use <see cref="ClearExpiration" /> to remove an existing expiration.
		/// </summary>
		[JsonIgnore]
		public DateTime? ExpireAtUtc { get; init; }

		/// <summary>
		/// The unique identifier of the submission.
		/// </summary>
		[JsonIgnore]
		public SubmissionId Id { get; init; }

		/// <summary>
		/// Set <see langword="true" /> to archive the submission or <see langword="false" /> to unarchive it. Leave unset to keep the submission's current state.
		/// </summary>
		[JsonPropertyName("archived")]
		public bool? IsArchived { get; init; }

		/// <summary>
		/// The name of the submission.
		/// </summary>
		public string? Name { get; init; }
	}

	/// <summary>
	/// The body written when <see cref="Request.ClearExpiration" /> is set: the request's members plus an explicit <c>"expire_at": null</c>.
	/// </summary>
	internal sealed class RequestBody {
		[JsonIgnore(Condition = JsonIgnoreCondition.Never), JsonPropertyName("expire_at")]
		public string? ExpireAt { get; init; }

		[JsonPropertyName("archived")]
		public bool? IsArchived { get; init; }

		public string? Name { get; init; }
	}

	/// <summary>
	/// Update submission response.
	/// </summary>
	public sealed class Response :
		ResponseBase<Response> {
		/// <summary>
		/// The updated submission, with its documents. The response carries no events.
		/// </summary>
		public Submission? Submission { get; init; }
	}
}

//	================================================================================
//	Validators
//	================================================================================

file sealed class RequestValidator :
	AbstractValidator<Request> {
	public RequestValidator() {
		RuleFor(r => r.ClearExpiration).Must(c => !c).When(r => r.ExpireAtUtc.HasValue).WithMessage("'ExpireAtUtc' and 'ClearExpiration' cannot both be set.");
		RuleFor(r => r.Id).NotEmpty();
	}
}