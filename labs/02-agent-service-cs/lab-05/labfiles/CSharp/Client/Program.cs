using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Client code that connects to the routing agent
/// </summary>

// Load configuration
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration configuration = builder.Build();
var serverUrl = configuration["ServerUrl"] ?? "localhost";
var routingAgentPort = configuration["RoutingAgentPort"] ?? "5003";

var httpClient = new HttpClient();
var baseUrl = $"http://{serverUrl}:{routingAgentPort}";

Console.WriteLine("A2A Agent Client");
Console.WriteLine("================");
Console.WriteLine("Enter a prompt for the agent. Type 'quit' to exit.\n");

while (true)
{
    Console.Write("User: ");
    var userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }

    if (userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    try
    {
        var response = await SendPromptAsync(userInput);
        Console.WriteLine($"Agent: {response}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n");
    }
}

async Task<string> SendPromptAsync(string prompt)
{
    var url = $"{baseUrl}/message";
    var payload = new { message = prompt };
    
    var response = await httpClient.PostAsJsonAsync(url, payload);
    
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("response").GetString() ?? "No response from agent.";
    }
    else
    {
        return $"Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}";
    }
}
