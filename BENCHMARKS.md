# Benchmarks

#### 2026-10-07 (.NET 10, mocked HTTP, performance pass)

> Before and after the #21 performance and allocation pass, both on the same machine and the same fixtures. The suite now covers the 2.0.0 slices and the webhook parser, and every class reports `Allocated`. The "before" column is the 2.0.0 code as it stood after #20, so it is not comparable with the 2026-10-07 baseline below, which predates the API-alignment work.

- BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
- Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
- .NET SDK 10.0.401
  - [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  - DefaultJob : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

###### Submissions

| Method                | Mean (before) | Mean (after) | Allocated (before) | Allocated (after) |
| --------------------- | ------------: | -----------: | -----------------: | ----------------: |
| Create                |      17.51 us |     12.79 us |           15.27 KB |          13.94 KB |
| CreateFromPdf         |      26.29 us |     23.22 us |           21.59 KB |          19.25 KB |
| List                  |      20.44 us |     15.63 us |           21.04 KB |          17.36 KB |
| UpdateClearExpiration |      15.88 us |     12.14 us |           16.28 KB |          14.02 KB |

###### Submitters

| Method | Mean (before) | Mean (after) | Allocated (before) | Allocated (after) |
| ------ | ------------: | -----------: | -----------------: | ----------------: |
| List   |      10.72 us |      8.03 us |           11.99 KB |          10.30 KB |

###### Templates

| Method             | Mean (before) | Mean (after) | Allocated (before) | Allocated (after) |
| ------------------ | ------------: | -----------: | -----------------: | ----------------: |
| CreateFromHtml     |      15.65 us |     14.60 us |           16.05 KB |          13.70 KB |
| List               |      17.05 us |     15.64 us |           19.78 KB |          16.67 KB |
| ListWithParameters |      18.67 us |     17.28 us |           21.61 KB |          17.77 KB |

###### Webhooks

| Method                     | Mean (before) | Mean (after) | Allocated (before) | Allocated (after) |
| -------------------------- | ------------: | -----------: | -----------------: | ----------------: |
| ParseFormCompletedString   |      12.18 us |     10.91 us |            4.99 KB |           4.73 KB |
| ParseFormCompletedUtf8     |      14.99 us |     10.64 us |            4.99 KB |           4.73 KB |
| ParseTemplateCreatedString |      14.73 us |     13.37 us |            4.45 KB |           4.16 KB |
| ParseTemplateCreatedUtf8   |      17.98 us |     13.10 us |            4.45 KB |           4.16 KB |

###### Optimisations

- **Response body read as UTF-8 bytes:** a body with no charset or a UTF-8 charset is read fully with `ReadAsByteArrayAsync`, checked to be valid UTF-8 without allocating, and deserialized from the bytes instead of being decoded to a UTF-16 string first; a valid UTF-8 body binds exactly as before. Any other charset, a UTF-16 or UTF-32 byte order mark, or bytes that are not valid UTF-8 are decoded through `ReadAsStringAsync` as before, so invalid bytes still read as U+FFFD. This is the largest allocation saving, about one copy of the body per call: Submissions `List` 3.1 KB less, Templates `List` 2.7 KB less, `CreateFromPdf` 2.2 KB less, Submitters `List` 1.3 KB less (each net of the builder and converter savings below).
- **Create-submission body read in one pass:** the created-submitters reader looks at the first token and deserializes the array once, instead of building a `JsonDocument` and deserializing the array from it. Measured as an A/B on the final code: Submissions `Create` 14.86 us to 12.51 us, 14.01 KB to 13.94 KB.
- **List endpoints built in one cached builder:** the three list requests append their query parameters to a thread-cached `StringBuilder` instead of a `HashSet`, an interpolated string per parameter and a join. Templates `ListWithParameters` 16.17 KB to 15.16 KB and 17.68 us to 16.86 us; each parameterless `List` 0.27 KB less. A new `StringBuilder` per call was tried first and added about 0.05 KB to the parameterless calls, so it was not kept.
- **Enum converters match without allocating:** the fourteen converters compare the token with `Utf8JsonReader.ValueTextEquals` against UTF-8 literals instead of allocating it with `GetString()`; only an unmatched token still calls `GetString()`, so unknown, null and invalid tokens behave as before. Allocations fell on every benchmark that reads an enum (Submissions `List` 14.51 KB to 14.21 KB, `CreateFromPdf` 17.23 KB to 17.05 KB, webhooks 4.99 KB to 4.80 KB and 4.45 KB to 4.23 KB); time changes were within noise.
- **Webhook payload parsed once:** `DocuSealWebhook.Parse` reads the envelope in one forward `Utf8JsonReader` pass and then deserializes the payload once, instead of building a `JsonDocument` and deserializing from it; the string overload transcodes into a pooled buffer. UTF-8 overload 14.71 us to 10.71 us (`form.completed`) and 17.72 us to 13.03 us (`template.created`); string overload 11.97 us to 10.92 us and 14.27 us to 13.11 us; 0.07 KB less each.




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
