using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Arex388.DocuSeal.Benchmarks.Benchmarks;

[SimpleJob, MemoryDiagnoser]
public class TemplatesBenchmarks
{
    private readonly IDocuSealClient _docuSeal;

    public TemplatesBenchmarks()
    {
        var services = new ServiceCollection().AddDocuSeal(new DocuSealClientOptions
        {
            AuthorizationToken = Config.AuthorizationToken
        }).BuildServiceProvider();

        _docuSeal = services.GetRequiredService<IDocuSealClient>();
    }

    [Benchmark]
    public Task<ListTemplates.Response> List() => _docuSeal.ListTemplatesAsync();
}