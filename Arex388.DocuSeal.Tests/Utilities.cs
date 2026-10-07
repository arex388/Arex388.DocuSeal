namespace Arex388.DocuSeal.Tests;

internal static class Utilities {
	/// <summary>
	/// The sample PDF, copied to the output directory by the project file.
	/// </summary>
	public static readonly FileInfo DocuSealFile = new(Path.Combine(AppContext.BaseDirectory, "DocuSeal.pdf"));

	public static Task<CreateSubmission.Response> CreateSubmissionAsync(
		IDocuSealClient docuSeal,
		Template template) => docuSeal.CreateSubmissionAsync(new CreateSubmission.Request {
			TemplateId = template.Id,
			Submitters = [
				new CreateSubmission.RequestSubmitter {
					Email = Config.Email1
				},
				new CreateSubmission.RequestSubmitter {
					Email = Config.Email2
				}
			]
		});

	public static async Task<CreateTemplate.Response> CreateTemplateAsync(
		IDocuSealClient docuSeal) {
		var fileBytes = await File.ReadAllBytesAsync(DocuSealFile.FullName);

		return await docuSeal.CreateTemplateAsync(new CreateTemplate.Request {
			Endpoint = CreateTemplate.Endpoints.Pdf,
			Name = DocuSealFile.Name,
			Documents = [
				new CreateTemplate.RequestDocument {
					Fields = [
						new CreateTemplate.RequestDocumentField {
							Areas = [
								new CreateTemplate.RequestDocumentFieldArea {
									Height = .06M,
									Page = 1,
									Width = .335M,
									X = .42M,
									Y = .15M
								}
							],
							Name = "Signature",
							Role = "Signer #1",
							Type = FieldType.Signature
						}
					],
					FileBase64 = Convert.ToBase64String(fileBytes),
					Name = DocuSealFile.Name
				}
			]
		});
	}
}