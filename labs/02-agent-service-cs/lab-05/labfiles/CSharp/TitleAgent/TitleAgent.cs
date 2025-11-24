using Microsoft.Extensions.Configuration;

// Add references


// Main entry point
var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var config = builder.Configuration.Get<AppConfig>() 
    ?? throw new InvalidOperationException("Failed to load configuration");

var host = config.ServerUrl;
var port = config.TitleAgentPort;

// Create the agents client


// Create the title agent


// Define agent skills


// Create agent card


// Build and run the application
builder.WebHost.UseUrls($"http://{host}:{port}");
var app = builder.Build();

// Health check endpoint
app.MapGet("/health", () => "AI Foundry Title Agent is running!");

// TODO: Add A2A endpoints when implementing the full solution

Console.WriteLine($"Title Agent starting on http://{host}:{port}");
app.Run();

/// <summary>
/// Configuration class for the application
/// </summary>
public class AppConfig
{
    public string ProjectConnectionString { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o";
    public string ServerUrl { get; set; } = "localhost";
    public int TitleAgentPort { get; set; } = 5001;
    public int OutlineAgentPort { get; set; } = 5002;
    public int RoutingAgentPort { get; set; } = 5003;
}
