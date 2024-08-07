using Arex388.DocuSeal.Benchmarks;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Submissions>();
BenchmarkRunner.Run<Submitters>();
BenchmarkRunner.Run<Templates>();