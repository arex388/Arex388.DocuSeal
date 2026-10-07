# Benchmarks

#### 2026-10-07 (.NET 10, mocked HTTP)

> From this entry on, the benchmarks run against mocked HTTP (`MockHttpMessageHandler` serving the shared `TestData/Responses` fixtures), so the numbers measure the client's own cost: request building, serialization, and response deserialization. They are not comparable to the 2024-09-23 entry below, which measured live API calls and was dominated by network latency.

- BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
- Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
- .NET SDK 10.0.401
  - [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  - DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

###### Submissions

| Method |      Mean | Allocated |
| ------ | --------: | --------: |
| List   | 13.756 us |  10.05 KB |

###### Submitters

| Method |     Mean | Allocated |
| ------ | -------: | --------: |
| List   | 6.986 us |   6.27 KB |

###### Templates

| Method |      Mean | Allocated |
| ------ | --------: | --------: |
| List   | 15.516 us |  10.94 KB |




#### 2024-09-23 (.NET 8, live API)

- BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
- Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
- .NET SDK 8.0.400
  - [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  - DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

###### Submissions

| Method |     Mean | Allocated |
| ------ | -------: | --------: |
| List   | 100.5 ms |  21.63 KB |

###### Submitters

| Method |     Mean | Allocated |
| ------ | -------: | --------: |
| List   | 104.4 ms |  11.87 KB |

###### Templates

| Method |     Mean | Allocated |
| ------ | -------: | --------: |
| List   | 84.27 ms |   7.47 KB |
