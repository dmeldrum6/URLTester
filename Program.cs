using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteLoadTest
{
    public class LoadTestResult
    {
        public int Iteration { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public double SuccessRate => TotalRequests > 0 ? (double)SuccessCount / TotalRequests * 100 : 0;
        public int TotalRequests => SuccessCount + FailureCount;
    }

    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly object lockObject = new object();

        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: URLTester <URL> <NumberOfConnections> <NumberOfIterations>");
                Console.WriteLine("Example: URLTester https://example.com 10 5");
                return;
            }

            var url = args[0];

            // Validate URL format
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validatedUri) ||
                (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
            {
                Console.WriteLine("Error: Invalid URL format. Please use http:// or https://");
                return;
            }

            if (!int.TryParse(args[1], out int numberOfConnections) || numberOfConnections <= 0)
            {
                Console.WriteLine("Error: Number of connections must be a positive integer.");
                return;
            }

            if (!int.TryParse(args[2], out int numberOfIterations) || numberOfIterations <= 0)
            {
                Console.WriteLine("Error: Number of iterations must be a positive integer.");
                return;
            }

            // Reasonable limits to prevent abuse
            if (numberOfConnections > 1000)
            {
                Console.WriteLine("Warning: High connection count. Consider using smaller values to avoid overwhelming the target server.");
                Console.WriteLine("Continue? (y/N)");
                var response = Console.ReadLine();
                if (response?.ToLower() != "y" && response?.ToLower() != "yes")
                {
                    return;
                }
            }

            // Configure HttpClient
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "LoadTester/1.0");

            try
            {
                await PerformIterativeLoadTest(validatedUri.ToString(), numberOfConnections, numberOfIterations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        static async Task PerformIterativeLoadTest(string url, int numberOfConnections, int numberOfIterations)
        {
            Console.WriteLine($"Load Test Configuration:");
            Console.WriteLine($"Target URL: {url}");
            Console.WriteLine($"Connections per iteration: {numberOfConnections}");
            Console.WriteLine($"Number of iterations: {numberOfIterations}");
            Console.WriteLine($"Total requests: {numberOfConnections * numberOfIterations}");
            Console.WriteLine(new string('=', 60));

            var results = new List<LoadTestResult>();
            var overallStopwatch = Stopwatch.StartNew();

            for (int iteration = 1; iteration <= numberOfIterations; iteration++)
            {
                Console.WriteLine($"\n--- Iteration {iteration}/{numberOfIterations} ---");
                var result = await PerformLoadTest(url, numberOfConnections, iteration);
                results.Add(result);

                Console.WriteLine($"Iteration {iteration}: {result.SuccessCount}/{result.TotalRequests} successful " +
                                $"({result.SuccessRate:F1}%) in {result.ElapsedMilliseconds}ms");

                // Add delay between iterations (except for the last one)
                if (iteration < numberOfIterations)
                {
                    Console.WriteLine("Waiting 2 seconds before next iteration...");
                    await Task.Delay(2000);
                }
            }

            overallStopwatch.Stop();
            PrintSummaryReport(results, overallStopwatch.Elapsed);
        }

        static async Task<LoadTestResult> PerformLoadTest(string url, int numberOfConnections, int iterationNumber)
        {
            var tasks = new List<Task>();
            int successCount = 0;
            int failureCount = 0;
            var stopwatch = Stopwatch.StartNew();

            // Use SemaphoreSlim to control concurrent connections
            var semaphore = new SemaphoreSlim(Math.Min(numberOfConnections, 50)); // Limit concurrent connections

            for (int i = 0; i < numberOfConnections; i++)
            {
                int connectionIndex = i + 1;
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            Interlocked.Increment(ref successCount);
                            Console.WriteLine($"  ✓ Connection {connectionIndex}: {response.StatusCode} ({response.ReasonPhrase})");
                        }
                        else
                        {
                            Interlocked.Increment(ref failureCount);
                            Console.WriteLine($"  ✗ Connection {connectionIndex}: {response.StatusCode} ({response.ReasonPhrase})");
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Interlocked.Increment(ref failureCount);
                        Console.WriteLine($"  ✗ Connection {connectionIndex}: Timeout");
                    }
                    catch (HttpRequestException ex)
                    {
                        Interlocked.Increment(ref failureCount);
                        Console.WriteLine($"  ✗ Connection {connectionIndex}: Network error - {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failureCount);
                        Console.WriteLine($"  ✗ Connection {connectionIndex}: {ex.GetType().Name} - {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            return new LoadTestResult
            {
                Iteration = iterationNumber,
                SuccessCount = successCount,
                FailureCount = failureCount,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }

        static void PrintSummaryReport(List<LoadTestResult> results, TimeSpan totalTime)
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("LOAD TEST SUMMARY REPORT");
            Console.WriteLine(new string('=', 60));

            var totalRequests = results.Sum(r => r.TotalRequests);
            var totalSuccesses = results.Sum(r => r.SuccessCount);
            var totalFailures = results.Sum(r => r.FailureCount);
            var overallSuccessRate = totalRequests > 0 ? (double)totalSuccesses / totalRequests * 100 : 0;

            Console.WriteLine($"Total Iterations: {results.Count}");
            Console.WriteLine($"Total Requests: {totalRequests}");
            Console.WriteLine($"Total Successes: {totalSuccesses}");
            Console.WriteLine($"Total Failures: {totalFailures}");
            Console.WriteLine($"Overall Success Rate: {overallSuccessRate:F2}%");
            Console.WriteLine($"Total Execution Time: {totalTime.TotalSeconds:F2} seconds");
            Console.WriteLine($"Average Requests/Second: {(totalRequests / totalTime.TotalSeconds):F2}");

            if (results.Count > 1)
            {
                var avgIterationTime = results.Average(r => r.ElapsedMilliseconds);
                var minIterationTime = results.Min(r => r.ElapsedMilliseconds);
                var maxIterationTime = results.Max(r => r.ElapsedMilliseconds);

                Console.WriteLine($"\nIteration Performance:");
                Console.WriteLine($"Average Time: {avgIterationTime:F0}ms");
                Console.WriteLine($"Fastest Time: {minIterationTime}ms");
                Console.WriteLine($"Slowest Time: {maxIterationTime}ms");

                // Show any iterations with significantly different performance
                var failedIterations = results.Where(r => r.SuccessRate < 90).ToList();
                if (failedIterations.Any())
                {
                    Console.WriteLine($"\nIterations with <90% success rate:");
                    foreach (var iteration in failedIterations)
                    {
                        Console.WriteLine($"  Iteration {iteration.Iteration}: {iteration.SuccessRate:F1}% success rate");
                    }
                }
            }

            Console.WriteLine(new string('=', 60));
        }
    }
}