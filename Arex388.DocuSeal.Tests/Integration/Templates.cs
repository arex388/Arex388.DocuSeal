using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Tests;

public sealed class Templates {
	private readonly ITestOutputHelper _console;
	private readonly IDocuSealClient _docuSeal;
	private readonly FileInfo _docuSealFile;

	public Templates(
		ITestOutputHelper console) {
		var services = new ServiceCollection().AddDocuSealClient(new DocuSealClientOptions {
			AuthorizationToken = Config.AuthorizationToken1
		}).BuildServiceProvider();

		_console = console;
		_docuSeal = services.GetRequiredService<IDocuSealClient>();

		var directory = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net8.0", null);

		_docuSealFile = new FileInfo($@"{directory}\DocuSeal.pdf");
	}

	[Fact]
	public async Task Archive_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var created = await _docuSeal.CreateTemplateAsync(_docuSealFile);

		//	========================================================================
		//	Act
		//	========================================================================

		var archived = await _docuSeal.ArchiveTemplateAsync(created.Template!.Id);

		//	========================================================================
		//	Assert
		//	========================================================================

		archived.Errors.Should().BeEmpty();
		archived.Success.Should().BeTrue();
	}

	[Fact]
	public async Task Clone_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var created = await _docuSeal.CreateTemplateAsync(_docuSealFile);

		//	========================================================================
		//	Act
		//	========================================================================

		var cloned = await _docuSeal.CloneTemplateAsync(new CloneTemplate.Request {
			Id = created.Template!.Id
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		cloned.Errors.Should().BeEmpty();
		cloned.Success.Should().BeTrue();
		cloned.Template.Should().NotBeNull();
		cloned.Template?.Name.Should().Be($"{created.Template.Name} (Clone)");

		await _docuSeal.ArchiveTemplateAsync(created.Template.Id);
		await _docuSeal.ArchiveTemplateAsync(cloned.Template!.Id);
	}

	[Fact]
	public async Task Create_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	========================================================================
		//	Act
		//	========================================================================

		var created = await _docuSeal.CreateTemplateAsync(_docuSealFile);

		//	========================================================================
		//	Assert
		//	========================================================================

		created.Errors.Should().BeEmpty();
		created.Success.Should().BeTrue();
		created.Template.Should().NotBeNull();

		await _docuSeal.ArchiveTemplateAsync(created.Template!.Id);
	}

	[Fact]
	public async Task Get_Fails() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		//	========================================================================
		//	Act
		//	========================================================================

		var gotten = await _docuSeal.GetTemplateAsync(new TemplateId(0));

		//	========================================================================
		//	Assert
		//	========================================================================

		gotten.Errors.Count.Should().Be(1);
		gotten.Success.Should().BeFalse();
		gotten.Template.Should().BeNull();
	}

	[Fact]
	public async Task Get_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var created = await _docuSeal.CreateTemplateAsync(_docuSealFile);

		//	========================================================================
		//	Act
		//	========================================================================

		var gotten = await _docuSeal.GetTemplateAsync(created.Template!.Id);

		//	========================================================================
		//	Assert
		//	========================================================================

		gotten.Template.Should().BeEquivalentTo(created.Template);

		await _docuSeal.ArchiveTemplateAsync(created.Template!.Id);
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

		var listed = await _docuSeal.ListTemplatesAsync(new ListTemplates.Request {
			Take = take
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		listed!.Errors.Count.Should().Be(errorsCount);
		listed.Success.Should().Be(success);
		listed.Templates.Count.Should().Be(listed.Pagination.Count);
	}

	[Fact]
	public async Task Merge_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var created1 = await _docuSeal.CreateTemplateAsync(_docuSealFile);
		var created2 = await _docuSeal.CreateTemplateAsync(_docuSealFile);

		//	========================================================================
		//	Act
		//	========================================================================

		var merged = await _docuSeal.MergeTemplatesAsync(new MergeTemplates.Request {
			Ids = [
				created1.Template!.Id,
				created2.Template!.Id
			]
		});

		//	========================================================================
		//	Assert
		//	========================================================================

		merged.Errors.Should().BeEmpty();
		merged.Success.Should().BeTrue();
		merged.Template.Should().NotBeNull();
		merged.Template?.Name.Should().Be($"{created1.Template.Name} (Merged)");

		await _docuSeal.ArchiveTemplateAsync(created1.Template.Id);
		await _docuSeal.ArchiveTemplateAsync(created2.Template.Id);
		await _docuSeal.ArchiveTemplateAsync(merged.Template!.Id);
	}

	[Fact]
	public async Task Update_Succeeds() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var created = await _docuSeal.CreateTemplateAsync(_docuSealFile);

		//	========================================================================
		//	Act
		//	========================================================================

		var updated = await _docuSeal.UpdateTemplateAsync(new UpdateTemplate.Request {
			Id = created.Template!.Id,
			Name = "DocuSeal (Updated)"
		});
		var gotten = await _docuSeal.GetTemplateAsync(created.Template.Id);

		//	========================================================================
		//	Assert
		//	========================================================================

		updated.Errors.Should().BeEmpty();
		updated.Success.Should().BeTrue();

		gotten.Errors.Should().BeEmpty();
		gotten.Success.Should().BeTrue();
		gotten.Template.Should().NotBeNull();
		gotten.Template!.Name.Should().Be("DocuSeal (Updated)");

		await _docuSeal.ArchiveTemplateAsync(created.Template.Id);
	}

	[Fact]
	public async Task Update_TemplateDocuments() {
		//	========================================================================
		//	Arrange
		//	========================================================================

		var created = await _docuSeal.CreateTemplateAsync(_docuSealFile);
		var fileBytes = await File.ReadAllBytesAsync(_docuSealFile.FullName);

		//	========================================================================
		//	Act
		//	========================================================================

		var updated = await _docuSeal.UpdateTemplateDocumentsAsync(new UpdateTemplateDocuments.Request {
			Documents = [
				new UpdateTemplateDocuments.RequestDocument {
					Name = "2nd Document",
					FileBase64 = Convert.ToBase64String(fileBytes)
				}
			],
			Id = created.Template!.Id
		});
		var gotten = await _docuSeal.GetTemplateAsync(created.Template.Id);

		//	========================================================================
		//	Assert
		//	========================================================================

		updated.Errors.Should().BeEmpty();
		updated.Success.Should().BeTrue();

		gotten.Errors.Should().BeEmpty();
		gotten.Success.Should().BeTrue();
		gotten.Template.Should().NotBeNull();
		gotten.Template!.Documents.Count.Should().Be(2);

		await _docuSeal.ArchiveTemplateAsync(created.Template.Id);
	}
}