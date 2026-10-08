# Arex388.DocuSeal

Arex388.DocuSeal is a highly opinionated .NET Standard 2.0 library for the [DocuSeal.co](https://www.docuseal.co/docs/api) API. It is intended to be an easy, well structured, and highly performant client for interacting with the DocuSeal.co API for sending documents for e-signatures. It can be used in applications interacting with a single account using `IDocuSealClient`, or with applications interacting with multiple accounts using `IDocuSealClientFactory`. 

As noted above, it is highly opinionated. The [API documentation](https://www.docuseal.co/docs/api) is not very clear, and there's redundancies, ambiguities, and object properties that I have no idea why they're there or what they stand for. While it has been one of the better documented APIs I've worked with, there was still some questionable design decisions about it. I've attempted to normalize the ambiguities, and to ignore the redundancies with this client.

- [Releases](https://github.com/arex388/Arex388.DocuSeal/releases)
- [Benchmarks](BENCHMARKS.md)



#### Dependency Injection

To configure dependency injection use the `AddDocuSeal()` extensions on `IServiceCollection`. There are two signatures, with and without passing in a `DocuSealClientOptions` object. If the options object is passed to the extension, it will register `IDocuSealClient` for use with a single account, otherwise it will register `IDocuSealClientFactory` for use with multiple accounts.

Accounts on DocuSeal's EU region set `Region = DocuSealRegion.Eu` on the `DocuSealClientOptions` (it defaults to `DocuSealRegion.Global`), which sends requests to `api.docuseal.eu` instead of `api.docuseal.com`; the factory caches clients per authorization token and region.



#### How to Use

For a single account, inject the `IDocuSealClient`.

```c#
private readonly IDocuSealClient _docuSeal;

_ = await _docuseal.ListTemplatesAsync();
```



For multiple accounts, inject the `IDocuSealClientFactory` to create an instance per account.

```c#
private readonly IDocuSealClientFactory _docuSealFactory;

var docuseal = _docuSealFactory.CreateClient(new DocuSealClientOptions {
    AuthorizationToken = "Your authorization token from DocuSeal.co"
});

_ = await docuseal.ListTemplatesAsync();
```



The client provides methods for interacting with Templates, Submissions, and Submitters using the following methods:

###### Templates

- `ArchiveTemplateAsync()` - Archive a template so it can't be used. This is essentially a soft delete.
- `CloneTemplateAsync()` - Clone a template.
- `CreateTemplateAsync()` - Create a new template.
- `CreateTemplateFromHtmlAsync()` - Create a new template from HTML with field tags.
- `GetTemplateAsync()` - Get an existing template.
- `ListTemplatesAsync()` - List all templates, with archived templates hidden by default.
- `MergeTemplatesAsync()` - Merge two or more templates into one.
- `UpdateTemplateAsync()` - Update a template.
- `UpdateTemplateDocumentsAsync()` - Update a template's documents.



###### Submissions

- `ArchiveSubmissionAsync()` - Archive a submission so it can't be used. This is essentially a soft delete.
- `CreateSubmissionAsync()` - Create a new submission for a template. The response carries the new submission's id (`SubmissionId`) and one `Submitter` per submitter in the request (`Submitters`), each with the `EmbedSrc` of its signing form. It does not carry the submission itself, so call `GetSubmissionAsync()` with the id when you need it.
- `GetSubmissionAsync()` - Get an existing submission.



###### Submitters

- `GetSubmitterAsync()` - Get a submitter for a submission.
- `ListSubmittersAsync()` - List the submitters for a submission.
- `UpdateSubmitterAsync()` - Update a submitter. The response carries the updated submitter, with its `EmbedSrc`.



#### Hoping for API Improvements

For the most part the API is one of the most well structured ones I've built a client for, and the [OpenAPI spec](https://console.docuseal.com/openapi.yml) has since settled most of what used to bother me. Creating a submission returns the submitters of the one submission that was created, not a submission that sometimes comes back as an array: in each of them `id` is the submitter's id and `submission_id` is the submission's id, which is why `CreateSubmission.Response` exposes `SubmissionId` and `Submitters` and `Submission.Id` is simply the `id` of the submission body. The status values, and every other enum, are defined, so the client types them (`SubmissionStatus`, `SubmitterStatus`, and the rest). It still has a couple of flaws:

- You can't ever delete something in DocuSeal, you can only archive it, which is just a soft delete. I would have preferred it if deleting was actually deleting, and archiving was just a specific update. The API can at least unarchive now, with an update that sets `archived` to `false`.
- Responses have properties that just don't seem to be relevant to anything. I've kept the majority, simply because I wasn't sure if users of this library might find them useful or not.

I'm sure there's something I've missed, but that's all I can remember now that I am at the end of two weeks of building this client. Hopefully, the DocuSeal API will improve in the future to address these, in my opinion, flaws.

That being said, it's a night-and-day difference working with the DocuSeal API vs the DocuSign API. The DocuSign API is absolutely the most awful API I've ever worked with. 