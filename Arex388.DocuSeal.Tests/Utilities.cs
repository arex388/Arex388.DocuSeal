namespace Arex388.DocuSeal.Tests;

internal static class Utilities {
	private static readonly FileInfo _file;

	static Utilities() {
		var directory = Directory.GetCurrentDirectory().Replace(@"\bin\Debug\net8.0", null);

		_file = new FileInfo($@"{directory}\DocuSeal.pdf");
	}

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
		var fileBytes = await File.ReadAllBytesAsync(_file.FullName);

		return await docuSeal.CreateTemplateAsync(new CreateTemplate.Request {
			Endpoint = CreateTemplate.Endpoints.Pdf,
			Name = _file.Name,
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
					Name = _file.Name
				}
			]
		});
	}
}