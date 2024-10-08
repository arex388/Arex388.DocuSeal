﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Tests;

public sealed class SubmissionsTest {
	private readonly ITestOutputHelper _console;
	private readonly IDocuSealClient _docuSeal;

	public SubmissionsTest(
		ITestOutputHelper console) {
		var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken1
		}).BuildServiceProvider();

		_console = console;
		_docuSeal = services.GetRequiredService<IDocuSealClient>();
	}

	[Fact]
	public async Task Archive_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);
		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);

		_console.WriteLineWithHeader(nameof(template), template);
		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Act
		//	========================================================================

		var archived = await _docuSeal.ArchiveSubmissionAsync(created.Submission!.Id);

		_console.WriteLineWithHeader(nameof(archived), archived);

		//	========================================================================
		//	Assert
		//	========================================================================

		archived.Errors.Should().BeEmpty();
		archived.Success.Should().BeTrue();

		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}

	[Fact]
	public async Task Create_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);

		_console.WriteLineWithHeader(nameof(template), template);

		//	========================================================================
		//	Act
		//	========================================================================

		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);

		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Assert
		//	========================================================================

		created.Errors.Should().BeEmpty();
		created.Success.Should().BeTrue();

		await _docuSeal.ArchiveSubmissionAsync(created.Submission!.Id);
		await _docuSeal.ArchiveTemplateAsync(template.Template!.Id);
	}

	[Fact]
	public async Task Get_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var template = await Utilities.CreateTemplateAsync(_docuSeal);
		var created = await Utilities.CreateSubmissionAsync(_docuSeal, template.Template!);

		_console.WriteLineWithHeader(nameof(template), template);
		_console.WriteLineWithHeader(nameof(created), created);

		//	========================================================================
		//	Act
		//	========================================================================

		var gotten = await _docuSeal.GetSubmissionAsync(created.Submission!.Id);

		_console.WriteLineWithHeader(nameof(gotten), gotten);

		//	========================================================================
		//	Assert
		//	========================================================================

		gotten.Errors.Should().BeEmpty();
		gotten.Success.Should().BeTrue();
		gotten.Submission.Should().NotBeNull();

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

		var listed = await _docuSeal.ListSubmissionsAsync(new ListSubmissions.Request {
			Take = take
		});

		_console.WriteLineWithHeader(nameof(listed), listed);

		//	========================================================================
		//	Assert
		//	========================================================================

		listed.Errors.Count.Should().Be(errorsCount);
		listed.Success.Should().Be(success);
		listed.Submissions.Count.Should().Be(listed.Pagination.Count);
	}
}