# Arex388.DocuSeal

Arex388.DocuSeal is a highly opinionated .NET Standard 2.0 library for the [DocuSeal.co](https://www.docuseal.co/docs/api) API. It is intended to be an easy, well structured, and highly performant client for interacting with the DocuSeal.co API for sending documents for e-signatures. It can be used in applications interacting with a single account using `IDocuSealClient`, or with applications interacting with multiple accounts using `IDocuSealClientFactory`. 

As noted above, it is highly opinionated. The [API documentation](https://www.docuseal.co/docs/api) is not very clear, and there's redundancies, ambiguities, and object properties that I have no idea why they're there or what they stand for. While it has been one of the better documented APIs I've worked with, there was still some questionable design decisions about it. I've attempted to normalize the ambiguities, and to ignore the redundancies with this client.

- [Changelog](CHANGELOG.md)
- [Benchmarks](BENCHMARKS.md)



#### Dependency Injection

To configure dependency injection with ASP.NET and ASP.NET Core, use the `AddDocuSeal()` extensions on `IServiceCollection`. There are two signatures, with and without passing in a `DocuSealClientOptions` object. If the options object is passed to the extension, it will register `IDocuSealClient` for use with a single account, otherwise it will register `IDocuSealClientFactory` for use with multiple accounts.



#### How to Use

For multiple accounts, use the `IDocuSealClientFactory` to create an instance per account.

```c#
private readonly IDocuSealClientFactory _docuSealFactory;

var docuSeal = _docuSealFactory.CreateClient(new DocuSealClientOptions {
    AuthorizationToken = "Your authorization token from DocuSeal.co"
});
```



The client provides methods for interacting with Templates, Submissions, and Submitters using the following methods:

###### Templates

- `ArchiveTemplateAsync()` - Archive a template so it can't be used. This is essentially a soft delete.
- `CloneTemplateAsync()` - Clone a template.
- `CreateTemplateAsync()` - Create a new template.
- `GetTemplateAsync()` - Get an existing template.
- `ListTemplatesAsync()` - List all templates, with archived templates hidden by default.
- `MergeTemplatesAsync()` - Merge two or more templates into one.
- `UpdateTemplateAsync()` - Update a template.
- `UpdateTemplateDocumentsAsync()` - Update a template's documents.



###### Submissions

- `ArchiveSubmissionAsync()` - Archive a submission so it can't be used. This is essentially a soft delete.
- `CreateSubmissionAsync()` - Create a new submission for a template.
- `GetSubmissionAsync()` - Get an existing submission.



###### Submitters

- `GetSubmitterAsync()` - Get a submitter for a submission.
- `ListSubmittersAsync()` - List the submitters for a submission.
- `UpdateSubmitterAsync()` - Update a submitter.



#### Hoping for API Improvements

For the most part the API is one of the most well structured ones I've built a client for, but it still has some flaws:

- You can't ever delete something in DocuSeal, you can only archive it, which is just a soft delete. I would have preferred it if deleting was actually deleting, and archiving was just a specific update.
- When creating a submission, the API may return an array or of a single object or the single object. I don't understand why an array will ever be a possible return even though you can only create a single submission?...
- Return values for certain properties, such as a status, are not defined in the documentation.
- Responses have properties that just don't seem to be relevant to anything. I've kept the majority, simply because I wasn't sure if user's of this library might find them useful or not.
- One weird response structure took me a bit to figure out while implementing the integration tests. The create submission response will return an `id` and a `submission_id` property, with each one having a different value. Initially I just used the `id` property, but then when I would try to get a submission with the returned `id` all I got was 404 responses. Turns out the `submission_id` is the real `id` for the submission. That the `id` property actually is, I have no idea.

I'm sure there's something I've missed, but that's all I can remember now that I am at the end of two weeks of building this client. Hopefully, the DocuSeal API will improve in the future to address these, in my opinion, flaws.

That being said, it's a night-and-day difference working with the DocuSeal API vs the DocuSign API. The DocuSign API is absolutely the most awful API I've ever worked with. 