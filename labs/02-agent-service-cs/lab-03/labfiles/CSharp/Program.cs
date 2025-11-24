using Microsoft.Extensions.Configuration;

// Add references


// Clear the console
Console.Clear();

// Load configuration from appsettings.json
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration configuration = builder.Build();
string projectConnectionString = configuration["ProjectConnectionString"] 
    ?? throw new InvalidOperationException("ProjectConnectionString is required in appsettings.json");
string model = configuration["Model"] 
    ?? throw new InvalidOperationException("Model is required in appsettings.json");


// Connect to the agents client


    // Create an agent to prioritize support tickets



    // Create an agent to assign tickets to the appropriate team



    // Create an agent to estimate effort for a support ticket



    // Create connected agent tools for the support agents

    

    // Create an agent to triage support ticket processing by using connected agents
    
    

    // Use the agents to triage a support issue



    // Clean up

