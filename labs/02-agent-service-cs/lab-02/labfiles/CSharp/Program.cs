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

        // Connect to the Agent client


        // Define an agent that can use the custom functions


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
