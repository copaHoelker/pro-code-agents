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

// Agent instructions
string summarizerInstructions = """
    You are a feedback summarizer. 
    Condense the customer feedback into a single, neutral sentence that captures the main point.
    Be concise and objective.
    """;

string classifierInstructions = """
    You are a feedback classifier.
    Categorize the feedback into one of these categories:
    - Positive: Customer expresses satisfaction
    - Negative: Customer expresses dissatisfaction or reports a problem
    - Feature request: Customer suggests a new feature or improvement

    Respond with only the category name.
    """;

string actionInstructions = """
    You are an action recommender.
    Based on the feedback summary and classification, recommend a single appropriate follow-up action.
    Be brief and actionable.
    Examples: "Log as enhancement request for product backlog", "Escalate to support team", "Send thank you response"
    """;

// Connect to the agents client


    // Create agents



    // Initialize the current feedback



    // Run sequential orchestration



    // Display final results



    // Clean up
