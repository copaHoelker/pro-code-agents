using Microsoft.Extensions.Configuration;

// Add references


class Program
{
    static async Task Main(string[] args)
    {
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

        // Display the data to be analyzed
        string filePath = Path.Combine(AppContext.BaseDirectory, "data.txt");
        string data = await File.ReadAllTextAsync(filePath);
        Console.WriteLine(data);
        Console.WriteLine();

        // Connect to the Agent client


            // Upload the data file and create a CodeInterpreterTool


            // Define an agent that uses the CodeInterpreterTool


            // Create a thread for the conversation

        
            // Loop until the user types 'quit'
            while (true)
            {
                // Get input text
                Console.Write("Enter a prompt (or type 'quit' to exit): ");
                string? userPrompt = Console.ReadLine();
                
                if (string.IsNullOrEmpty(userPrompt) || userPrompt.ToLower() == "quit")
                {
                    if (userPrompt?.ToLower() == "quit")
                        break;
                    
                    Console.WriteLine("Please enter a prompt.");
                    continue;
                }

                // Send a prompt to the agent


                // Check the run status for failures

        
                // Show the latest response from the agent


            }

            // Get the conversation history

        
            // Clean up

    }
}
