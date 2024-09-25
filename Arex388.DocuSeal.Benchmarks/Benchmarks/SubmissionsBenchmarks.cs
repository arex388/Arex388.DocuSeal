using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class SubmissionsBenchmarks
{
    private readonly IDocuSealClient _docuSeal;

    public SubmissionsBenchmarks()
    {
        var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions
        {
            AuthorizationToken = Config.AuthorizationToken
        }).BuildServiceProvider();

        _docuSeal = services.GetRequiredService<IDocuSealClient>();
    }

    [Benchmark]
    public Task<ListSubmissions.Response> List() => _docuSeal.ListSubmissionsAsync();
}