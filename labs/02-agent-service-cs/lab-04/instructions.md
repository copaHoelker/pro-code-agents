---
lab:
    title: 'Develop a multi-agent solution with Microsoft Agent Framework'
    description: 'Learn to configure multiple agents to collaborate using the Microsoft Foundry Agent Service SDK'
---

# Develop a multi-agent solution

In this exercise, you'll practice using a sequential orchestration pattern with the Microsoft Foundry Agent Service SDK. You'll create a simple pipeline of three agents that work together to process customer feedback and suggest next steps. You'll create the following agents:

- The Summarizer agent will condense raw feedback into a short, neutral sentence.
- The Classifier agent will categorize the feedback as Positive, Negative, or a Feature request.
- Finally, the Recommended Action agent will recommend an appropriate follow-up step.

You'll learn how to use the Microsoft Foundry Agent Service SDK to break down a problem, route it through the right agents, and produce actionable results. Let's get started!

> **Tip**: The code used in this exercise is based on the Foundry SDK for C#. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

This exercise should take approximately **30** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Access the existing Foundry project

You'll use an existing Foundry project that has been pre-configured for this lab.

1. In a web browser, open the [Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Foundry** logo at the top left to navigate to the home page, which looks similar to the following image (close the **Help** pane if it's open):

   ![Screenshot of Foundry portal.](./_images/ai-foundry-home.png)

   > **Important**: Make sure the **New Foundry** toggle is _Off_ for this lab.

1. In the home page, select **All resources** from the left navigation pane.
1. Locate and select the project named **pro-code-agents-student**.
1. In the navigation pane on the left, select **Models and endpoints** to verify that the **gpt-4o** model is deployed and available.
1. In the navigation pane on the left, select **Overview** to see the main page for your project; which looks like this:

   ![Screenshot of a Azure AI project details in Foundry portal.](./_images/ai-foundry-project.png)

The project has several pre-deployed models available for use, including **gpt-4o**, **gpt-4o-mini**, **gpt-4.1-mini**, **gpt-5-mini**, and **text-embedding-ada-002**.

## Create an AI Agent client app

Now you're ready to create a client app that defines an agent and a custom function. The code files are provided in the labfiles folder.

### Prepare your development environment

You have three options for your development environment:

- **GitHub Codespaces**: A cloud-based development environment
- **Local Development in Dev Containers**: Using Docker and VS Code
- **Fallback VM**: Provided by your instructor if the above options are not available

Open a terminal in your chosen environment and navigate to the lab files.

1. Change to the working directory containing the code files for this lab:

   ```bash
   cd labfiles/CSharp
   ls -l
   ```

   The provided files include application code and a file for configuration settings.

### Configure the application settings

1. In the terminal, enter the following command to restore the NuGet packages:

   ```bash
   dotnet restore
   ```

1. Enter the following command to edit the configuration file that is provided:

   ```bash
   code appsettings.json
   ```

   The file is opened in a code editor.

1. In the configuration file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Foundry portal). Ensure that the Model variable is set to **gpt-4o**.

1. After you've replaced the placeholders, save your changes and close the code editor.

### Create AI agents

Now you're ready to create the agents for your multi-agent solution! Let's get started!

1. Enter the following command to edit the **Program.cs** file:

   ```bash
   code Program.cs
   ```

1. Review the code in the file, noting that it loads configuration settings.

1. At the top of the file under the comment **Add references**, add the following code to reference the namespaces you'll need to implement your agent:

   ```csharp
   // Add references
   using Azure.AI.Agents.Persistent;
   using Azure.Identity;
   ```

1. In the **main** code section, take a moment to review the agent instructions. These instructions define the behavior of each agent in the orchestration.

1. Add the following code under the comment **Connect to the agents client**:

   ```csharp
   // Connect to the agents client
   var agentsClient = new PersistentAgentsClient(
       projectConnectionString,
       new DefaultAzureCredential(new DefaultAzureCredentialOptions
       {
           ExcludeEnvironmentCredential = true,
           ExcludeManagedIdentityCredential = true
       })
   );
   ```

   Note that the **DefaultAzureCredential** object will allow your code to authenticate to your Azure account.

1. Add the following code under the comment **Create agents**:

   ```csharp
   // Create agents
   PersistentAgent summarizer = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: "summarizer-xxx",
       instructions: summarizerInstructions
   );
   Console.WriteLine($"Created summarizer agent: {summarizer.Id}");

   PersistentAgent classifier = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: "classifier-xxx",
       instructions: classifierInstructions
   );
   Console.WriteLine($"Created classifier agent: {classifier.Id}");

   PersistentAgent action = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: "action-xxx",
       instructions: actionInstructions
   );
   Console.WriteLine($"Created action agent: {action.Id}");
   ```

   > **Note**: To avoid naming conflicts with other students, use "xxx" as a suffix where `xxx` is the first three letters of your first name (e.g., "summarizer-ali" for Alice).

## Create a sequential orchestration

1. Find the comment **Initialize the current feedback** and add the following code:

   ```csharp
   // Initialize the current feedback
   string feedback = """
       I use the dashboard every day to monitor metrics, and it works well overall.
       But when I'm working late at night, the bright screen is really harsh on my eyes.
       If you added a dark mode option, it would make the experience much more comfortable.
       """;
   ```

1. Under the comment **Run sequential orchestration**, add the following code to process the feedback through each agent in sequence:

   ```csharp
   // Run sequential orchestration
   Console.WriteLine("\nProcessing feedback through sequential orchestration...\n");
   Console.WriteLine($"Original Feedback:\n{feedback}\n");
   Console.WriteLine(new string('-', 60));

   // Step 1: Summarize the feedback
   Console.WriteLine("\n[Step 1: Summarizer]");
   PersistentAgentThread thread1 = await agentsClient.Threads.CreateThreadAsync();
   await agentsClient.Messages.CreateMessageAsync(
       threadId: thread1.Id,
       role: MessageRole.User,
       content: $"Customer feedback: {feedback}"
   );
   ThreadRun run1 = await agentsClient.Runs.CreateRunAsync(thread: thread1, agent: summarizer);
   while (run1.Status == RunStatus.Queued || run1.Status == RunStatus.InProgress)
   {
       await Task.Delay(1000);
       run1 = await agentsClient.Runs.GetRunAsync(thread1.Id, run1.Id);
   }
   string summary = await GetLastAssistantMessage(agentsClient, thread1.Id);
   Console.WriteLine($"Summary: {summary}");

   // Step 2: Classify the feedback
   Console.WriteLine("\n[Step 2: Classifier]");
   PersistentAgentThread thread2 = await agentsClient.Threads.CreateThreadAsync();
   await agentsClient.Messages.CreateMessageAsync(
       threadId: thread2.Id,
       role: MessageRole.User,
       content: summary
   );
   ThreadRun run2 = await agentsClient.Runs.CreateRunAsync(thread: thread2, agent: classifier);
   while (run2.Status == RunStatus.Queued || run2.Status == RunStatus.InProgress)
   {
       await Task.Delay(1000);
       run2 = await agentsClient.Runs.GetRunAsync(thread2.Id, run2.Id);
   }
   string classification = await GetLastAssistantMessage(agentsClient, thread2.Id);
   Console.WriteLine($"Classification: {classification}");

   // Step 3: Recommend action
   Console.WriteLine("\n[Step 3: Recommended Action]");
   PersistentAgentThread thread3 = await agentsClient.Threads.CreateThreadAsync();
   await agentsClient.Messages.CreateMessageAsync(
       threadId: thread3.Id,
       role: MessageRole.User,
       content: $"Summary: {summary}\nClassification: {classification}"
   );
   ThreadRun run3 = await agentsClient.Runs.CreateRunAsync(thread: thread3, agent: action);
   while (run3.Status == RunStatus.Queued || run3.Status == RunStatus.InProgress)
   {
       await Task.Delay(1000);
       run3 = await agentsClient.Runs.GetRunAsync(thread3.Id, run3.Id);
   }
   string recommendedAction = await GetLastAssistantMessage(agentsClient, thread3.Id);
   Console.WriteLine($"Action: {recommendedAction}");
   ```

   This code creates a sequential pipeline that:
   1. Passes the raw feedback to the summarizer agent
   2. Takes the summarized output and passes it to the classifier agent
   3. Takes both the summary and classification and passes them to the action agent

1. At the end of the file, add the following helper function to retrieve the last assistant message from a thread:

   ```csharp
   // Helper function to get the last assistant message from a thread
   static async Task<string> GetLastAssistantMessage(PersistentAgentsClient client, string threadId)
   {
       var messages = client.Messages.GetMessagesAsync(
           threadId: threadId, 
           order: ListSortOrder.Descending
       );
       
       await foreach (var msg in messages)
       {
           foreach (var content in msg.ContentItems)
           {
               if (content is MessageTextContent textContent)
               {
                   return textContent.Text;
               }
           }
           break; // Only get the first (latest) message
       }
       return "No response received";
   }
   ```

1. Add the following code under the comment **Display final results**:

   ```csharp
   // Display final results
   Console.WriteLine("\n" + new string('=', 60));
   Console.WriteLine("SEQUENTIAL ORCHESTRATION RESULTS");
   Console.WriteLine(new string('=', 60));
   Console.WriteLine($"01 [user]\nCustomer feedback: {feedback.Trim()}");
   Console.WriteLine(new string('-', 60));
   Console.WriteLine($"02 [summarizer]\n{summary}");
   Console.WriteLine(new string('-', 60));
   Console.WriteLine($"03 [classifier]\n{classification}");
   Console.WriteLine(new string('-', 60));
   Console.WriteLine($"04 [action]\n{recommendedAction}");
   Console.WriteLine(new string('=', 60));
   ```

1. Add the following code under the comment **Clean up**:

   ```csharp
   // Clean up
   Console.WriteLine("\nCleaning up agents...");
   await agentsClient.Administration.DeleteAgentAsync(summarizer.Id);
   Console.WriteLine("Deleted summarizer agent.");
   await agentsClient.Administration.DeleteAgentAsync(classifier.Id);
   Console.WriteLine("Deleted classifier agent.");
   await agentsClient.Administration.DeleteAgentAsync(action.Id);
   Console.WriteLine("Deleted action agent.");
   ```

1. Save your changes to the code file. You can keep it open in case you need to edit the code to fix any errors.

### Run the app

Now you're ready to run your code and watch your AI agents collaborate.

1. In the terminal, enter the following command to build the application:

   ```bash
   dotnet build
   ```

1. Then run the application:

   ```bash
   dotnet run
   ```

   You should see some output similar to the following:

   ```output
   Processing feedback through sequential orchestration...

   Original Feedback:
       I use the dashboard every day to monitor metrics, and it works well overall.
       But when I'm working late at night, the bright screen is really harsh on my eyes.
       If you added a dark mode option, it would make the experience much more comfortable.

   ------------------------------------------------------------

   [Step 1: Summarizer]
   Summary: User requests a dark mode for better nighttime usability.

   [Step 2: Classifier]
   Classification: Feature request

   [Step 3: Recommended Action]
   Action: Log as enhancement request for product backlog.

   ============================================================
   SEQUENTIAL ORCHESTRATION RESULTS
   ============================================================
   01 [user]
   Customer feedback: I use the dashboard every day to monitor metrics...
   ------------------------------------------------------------
   02 [summarizer]
   User requests a dark mode for better nighttime usability.
   ------------------------------------------------------------
   03 [classifier]
   Feature request
   ------------------------------------------------------------
   04 [action]
   Log as enhancement request for product backlog.
   ============================================================

   Cleaning up agents...
   Deleted summarizer agent.
   Deleted classifier agent.
   Deleted action agent.
   ```

1. Optionally, you can try running the code using different feedback inputs by modifying the `feedback` variable in the code. Some examples:

   - `I reached out to your customer support yesterday because I couldn't access my account. The representative responded almost immediately, was polite and professional, and fixed the issue within minutes. Honestly, it was one of the best support experiences I've ever had.`

   - `The mobile app crashes every time I try to upload a photo. This is frustrating and makes the app unusable for sharing content.`

## Summary

In this exercise, you practiced sequential orchestration with the Microsoft Foundry Agent Service SDK, combining multiple agents into a single, streamlined workflow. Great work!
