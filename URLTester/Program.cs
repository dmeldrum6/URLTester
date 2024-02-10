using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebsiteLoadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: URLTester <URL> <NumberOfConnections>");
                return;
            }

            var url = args[0];
            if (!int.TryParse(args[1], out int numberOfConnections))
            {
                Console.WriteLine("Invalid number of connections.");
                return;
            }

            await PerformLoadTest(url, numberOfConnections);
        }

        static async Task PerformLoadTest(string url, int numberOfConnections)
        {
            var tasks = new List<Task>();
            var httpClient = new HttpClient();

            Console.WriteLine($"Starting load test on {url} with {numberOfConnections} connections...");

            for (int i = 0; i < numberOfConnections; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Connection {Task.CurrentId}: Success");
                        }
                        else
                        {
                            Console.WriteLine($"Connection {Task.CurrentId}: Failed with status code {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Connection {Task.CurrentId}: Exception occurred - {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("Load test completed.");
        }
    }
}