# Benchmarks

- BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
- Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
- .NET SDK 8.0.400
  - [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  - DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2




#### 2024-09-23 (.NET 8)

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
