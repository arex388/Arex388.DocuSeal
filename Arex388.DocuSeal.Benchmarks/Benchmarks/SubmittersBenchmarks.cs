using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class SubmittersBenchmarks
{
    private readonly IDocuSealClient _docuSeal;

    public SubmittersBenchmarks()
    {
        var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions
        {
            AuthorizationToken = Config.AuthorizationToken
        }).BuildServiceProvider();

        _docuSeal = services.GetRequiredService<IDocuSealClient>();
    }

    [Benchmark]
    public Task<ListSubmitters.Response> List() => _docuSeal.ListSubmittersAsync();
}