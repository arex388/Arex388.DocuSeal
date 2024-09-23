using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Tests;

public sealed class SubmittersTest {
	private readonly ITestOutputHelper _console;
	private readonly IDocuSealClient _docuSeal;

	public SubmittersTest(
		ITestOutputHelper console) {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken1
		}).BuildServiceProvider();

		_console = console;
		_docuSeal = services.GetRequiredService<IDocuSealClient>();
	}

	[Fact]
	public async Task Get_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);
		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);
		var submission = await _docuSeal.GetSubmissionAsync(created.Submission!.Id);

		_console.WriteLineWithHeader(nameof(template), template);
		_console.WriteLineWithHeader(nameof(created), created);
		_console.WriteLineWithHeader(nameof(submission), submission);

		//	========================================================================
		//	Act
		//	========================================================================

		var gotten = await _docuSeal.GetSubmitterAsync(submission.Submission!.Submitters[0].Id);

		_console.WriteLineWithHeader(nameof(gotten), gotten);

		//	========================================================================
		//	Assert
		//	========================================================================

		gotten.Errors.Should().BeEmpty();
		gotten.Success.Should().BeTrue();
		gotten.Submitter.Should().NotBeNull();

		await _docuSeal.ArchiveSubmissionAsync(created.Submission!.Id);
		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}

	[Theory]
	[InlineData(10, true, 0)]
	[InlineData(100, true, 0)]
	[InlineData(101, false, 1)]
	public async Task List_Succeeds(
		int take,
		bool success,
		int errorsCount) {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	========================================================================
		//	Act
		//	========================================================================

		var response = await _docuSeal.ListSubmittersAsync(new ListSubmitters.Request {
			Take = take
		});

		_console.WriteLineWithHeader(nameof(response), response);

		//	========================================================================
		//	Assert
		//	========================================================================

		response.Errors.Count.Should().Be(errorsCount);
		response.Success.Should().Be(success);
		response.Submitters.Count.Should().Be(response.Pagination.Count);
	}

	[Fact]
	public async Task Update_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);
		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);
		var submission = await _docuSeal.GetSubmissionAsync(created.Submission!.Id);
		var submitterId = submission.Submission!.Submitters[0].Id;
		var gotten = await _docuSeal.GetSubmitterAsync(submitterId);

		_console.WriteLineWithHeader(nameof(template), template);
		_console.WriteLineWithHeader(nameof(created), created);
		_console.WriteLineWithHeader(nameof(submission), submission);
		_console.WriteLineWithHeader(nameof(gotten), gotten);

		//	========================================================================
		//	Act
		//	========================================================================

		var updated = await _docuSeal.UpdateSubmitterAsync(new UpdateSubmitter.Request {
			Id = submitterId,
			Name = "Test Submitter"
		});

		_console.WriteLineWithHeader(nameof(updated), updated);

		//	========================================================================
		//	Assert
		//	========================================================================

		updated.Errors.Should().BeEmpty();
		updated.Success.Should().BeTrue();

		await _docuSeal.ArchiveSubmissionAsync(created.Submission!.Id);
		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}
}