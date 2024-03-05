using System.Diagnostics;
using System.Net.Http.Headers;

if (args is not [var url])
{
    Console.WriteLine("Missing command line arg");
    return 1;
}

Console.WriteLine($"Testing against {url}");
using var client = new HttpClient();
client.BaseAddress = new Uri(url);
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

const int totalRequests = 10000;
const int concurrentRequests = 20;

var requestsStarted = 0;
var requestsComplete = 0;
var tasks = new List<Task<HttpResponseMessage>>(concurrentRequests);
var total = Stopwatch.StartNew();
var lastUpdate = Stopwatch.StartNew();

while (requestsStarted < totalRequests)
{
    if (tasks.Count is concurrentRequests)
    {
        var completedTask = await Task.WhenAny(tasks);
        tasks.Remove(completedTask);
        requestsComplete += 1;
    }
    
    tasks.Add(client.GetAsync("weatherforecast"));
    requestsStarted += 1;

    if (lastUpdate.ElapsedMilliseconds >= 1000)
    {
        Console.WriteLine($"{requestsComplete} requests completed");
        lastUpdate.Restart();
    }
}

await Task.WhenAll(tasks);

Console.WriteLine($"{totalRequests} complete in {total.Elapsed:g}");
return 0;
