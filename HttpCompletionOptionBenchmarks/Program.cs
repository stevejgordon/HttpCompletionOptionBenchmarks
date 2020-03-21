using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpCompletionOptionBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<HttpClientBenchmarks>();
        }
    }

    // RESULTS
    // Reading the content and deserialising (as per the code below)

    // 5 Books
    //|                      Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
    //| WithoutHttpCompletionOption | 259.9 us | 4.70 us | 4.39 us | 0.9766 |     - |     - |   4.58 KB |
    //|    WithHttpCompletionOption | 265.8 us | 5.19 us | 9.22 us | 0.4883 |     - |     - |   3.93 KB |

    // 250 Books
    //|                      Method |     Mean |    Error |   StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------------------------- |---------:|---------:|---------:|--------:|------:|------:|----------:|
    //| WithoutHttpCompletionOption | 739.6 us | 14.69 us | 38.69 us | 19.5313 |     - |     - |  79.11 KB |
    //|    WithHttpCompletionOption | 670.9 us | 23.23 us | 23.85 us |  4.8828 |     - |     - |  20.01 KB |


    // Read stream but do not deserialise from it

    // 5 Books
    //|                      Method |     Mean |   Error |  StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------------------------- |---------:|--------:|--------:|-------:|------:|------:|----------:|
    //| WithoutHttpCompletionOption | 258.2 us | 4.61 us | 7.84 us | 0.4883 |     - |     - |   3.82 KB |
    //|    WithHttpCompletionOption | 258.6 us | 5.08 us | 4.75 us | 0.4883 |     - |     - |   3.16 KB |

    // 250 Books
    //|                      Method |     Mean |    Error |   StdDev |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
    //|---------------------------- |---------:|---------:|---------:|--------:|-------:|------:|----------:|
    //| WithoutHttpCompletionOption | 487.9 us | 11.52 us | 17.24 us | 15.6250 | 0.9766 |     - |  62.94 KB |
    //|    WithHttpCompletionOption | 406.2 us |  7.88 us |  7.38 us |  0.9766 |      - |     - |   4.24 KB |


    // Not consuming the content

    // 5 Books
    //|                      Method |     Mean |   Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------------------------- |---------:|--------:|---------:|-------:|------:|------:|----------:|
    //| WithoutHttpCompletionOption | 255.9 us | 5.01 us |  8.23 us | 0.4883 |     - |     - |   3.67 KB |
    //|    WithHttpCompletionOption | 259.5 us | 5.21 us | 11.09 us | 0.4883 |     - |     - |   3.07 KB |

    // 250 Books
    //|                      Method |     Mean |   Error |   StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------------------------- |---------:|--------:|---------:|--------:|------:|------:|----------:|
    //| WithoutHttpCompletionOption | 480.1 us | 9.36 us | 12.50 us | 15.6250 |     - |     - |  62.77 KB |
    //|    WithHttpCompletionOption | 411.8 us | 8.21 us | 14.17 us |  0.9766 |     - |     - |   4.15 KB |


    [MemoryDiagnoser]
    public class HttpClientBenchmarks
    {
        private HttpClient _httpClient;

        [GlobalSetup]
        public void Setup()
        {
            _httpClient = new HttpClient();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _httpClient.Dispose();
        }

        [Benchmark]
        public async Task WithoutHttpCompletionOption()
        {
            var response = await _httpClient.GetAsync("http://localhost:58815/books");

            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<List<Book>>(stream);
            }
        }

        public async Task TryFinallyExample()
        {
            var response = await _httpClient.GetAsync("http://localhost:58815/books", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            List<Book> data = null;

            try
            {
                if (response.Content is object)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    data = await JsonSerializer.DeserializeAsync<List<Book>>(stream);
                }
            }
            finally
            {
                response.Dispose();
            }

            if (data is object)
            {
                // intensive and slow processing of books list. We don't want this to delay releasing the connection.
            }
        }

        [Benchmark]
        public async Task WithHttpCompletionOption()
        {
            using var response = await _httpClient.GetAsync("http://localhost:58815/books", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<List<Book>>(stream);

                // do something with the data or return it
            }
        }

        // Simplified code
        [Benchmark]
        public async Task WithGetStreamAsync()
        {
            using var stream = await _httpClient.GetStreamAsync("http://localhost:58815/books");

            var data = await JsonSerializer.DeserializeAsync<List<Book>>(stream);
        }
    }
}
