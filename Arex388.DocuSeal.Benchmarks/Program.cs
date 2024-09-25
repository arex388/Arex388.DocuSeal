using Arex388.DocuSeal.Benchmarks.Benchmarks;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<SubmissionsBenchmarks>();
BenchmarkRunner.Run<SubmittersBenchmarks>();
BenchmarkRunner.Run<TemplatesBenchmarks>();