# Benchmarks

- BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4651/22H2/2022Update)
- Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
- .NET SDK 8.0.303
  - [Host]     : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2
  - DefaultJob : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2



#### 2024-08-06 (.NET8)

###### Submissions

| Method |     Mean |    Error |   StdDev | Allocated |
| ------ | -------: | -------: | -------: | --------: |
| List   | 96.76 ms | 1.790 ms | 1.990 ms |  20.16 KB |

###### Submitters

| Method |     Mean |   Error |  StdDev | Allocated |
| ------ | -------: | ------: | ------: | --------: |
| List   | 106.1 ms | 2.09 ms | 4.85 ms |  11.67 KB |

###### Templates

| Method |     Mean |    Error |   StdDev | Allocated |
| ------ | -------: | -------: | -------: | --------: |
| List   | 84.65 ms | 1.620 ms | 1.866 ms |   8.11 KB |



#### 2024-07-31 (.NET8)

###### Submissions

| Method |     Mean |    Error |   StdDev | Allocated |
| ------ | -------: | -------: | -------: | --------: |
| List   | 94.93 ms | 1.832 ms | 2.568 ms |  20.89 KB |

###### Submitters

| Method |     Mean |   Error |  StdDev | Allocated |
| ------ | -------: | ------: | ------: | --------: |
| List   | 105.6 ms | 1.73 ms | 2.84 ms |  12.19 KB |

###### Templates

| Method |     Mean |    Error |   StdDev | Allocated |
| ------ | -------: | -------: | -------: | --------: |
| List   | 99.17 ms | 1.664 ms | 1.475 ms |  32.19 KB |