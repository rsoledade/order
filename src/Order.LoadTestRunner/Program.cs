using System.Diagnostics;
using System.Text;
using System.Text.Json;

const string baseUrl = "https://localhost:7214";
const string endpoint = "/api/Orders/register-order";
const int totalRequests = 1000; // Adjust for your test (e.g., 200_000 for full day)
const int concurrency = 10;     // Number of concurrent tasks

var success = 0;
var fail = 0;
var totalTime = 0L;
var errors = new List<string>();

Console.WriteLine($"Starting load test: {totalRequests} requests, concurrency: {concurrency}");

var tasks = new List<Task>();
var throttler = new SemaphoreSlim(concurrency);

for (int i = 0; i < totalRequests; i++)
{
    await throttler.WaitAsync();
    tasks.Add(Task.Run(async () =>
    {
        var order = new
        {
            externalId = Guid.NewGuid().ToString(),
            products = new[]
            {
                new {
                    name = $"Product {Random.Shared.Next(1, 1000)}",
                    price = Random.Shared.Next(1, 100),
                    quantity = Random.Shared.Next(1, 5)
                }
            }
        };

        var json = JsonSerializer.Serialize(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new HttpClient();
            var response = await client.PostAsync($"{baseUrl}{endpoint}", content);
            sw.Stop();

            Interlocked.Add(ref totalTime, sw.ElapsedMilliseconds);

            if (response.IsSuccessStatusCode)
                Interlocked.Increment(ref success);
            else
            {
                Interlocked.Increment(ref fail);
                var resp = await response.Content.ReadAsStringAsync();
                lock (errors) { errors.Add(resp); }
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            Interlocked.Add(ref totalTime, sw.ElapsedMilliseconds);
            Interlocked.Increment(ref fail);
            lock (errors) { errors.Add(ex.Message); }
        }
        finally
        {
            throttler.Release();
        }
    }));
}

await Task.WhenAll(tasks);

Console.WriteLine("Load test finished.");
Console.WriteLine($"Total requests: {totalRequests}");
Console.WriteLine($"Success: {success}");
Console.WriteLine($"Failed: {fail}");
Console.WriteLine($"Average response time: {(totalRequests > 0 ? totalTime / totalRequests : 0)} ms");
if (fail > 0)
{
    Console.WriteLine("Sample errors:");
    foreach (var err in errors.Take(5))
        Console.WriteLine(err);
}
