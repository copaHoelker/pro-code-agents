---
lab:
    title: 'Use a custom function in an AI agent'
    description: 'Learn how to use functions to add custom capabilities to your agents.'
---

# Use a custom function in an AI agent

In this exercise you'll explore creating an agent that can use custom functions as a tool to complete tasks. You'll build a simple technical support agent that can collect details of a technical problem and generate a support ticket.

> **Tip**: The code used in this exercise is based on the for Microsoft Foundry SDK for C#. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Microsoft Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

This exercise should take approximately **30** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Access the existing Foundry project

You'll use an existing Foundry project that has been pre-configured for this lab.

1. In a web browser, open the [Foundry portal](https://ai.azure.com) at `https://ai.azure.com` and sign in using your Azure credentials. Close any tips or quick start panes that are opened the first time you sign in, and if necessary use the **Foundry** logo at the top left to navigate to the home page, which looks similar to the following image (close the **Help** pane if it's open):

   ![Screenshot of Foundry portal.](./_images/ai-foundry-home.png)

   > **Important**: Make sure the **New Foundry** toggle is _Off_ for this lab.

1. In the home page, select **All resources** from the left navigation pane.
1. Locate and select the project named **pro-code-agents-student**.
1. In the navigation pane on the left, select **Overview** to see the main page for your project; which looks like this:

   ![Screenshot of a Foundry project overview page.](./_images/ai-foundry-project.png)

1. Copy the **Foundry project endpoint** values to a notepad, as you'll use them to connect to your project in a client application.

The project has several pre-deployed models available for use, including **gpt-4o**, **gpt-4o-mini**, **gpt-4.1-mini**, **gpt-5-mini**, and **text-embedding-ada-002**.

## Develop an agent that uses function tools

Now that you've accessed your project in AI Foundry, let's develop an app that implements an agent using custom function tools.

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

1. Enter the following command to edit the configuration file that has been provided:

   ```bash
   code appsettings.json
   ```

   The file is opened in a code editor.

1. In the configuration file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Foundry portal) and ensure that the Model variable is set to **gpt-4o**.
1. After you've replaced the placeholder, save your changes and close the code editor.

### Define a custom function

1. Enter the following command to edit the code file that has been provided for your function code:

   ```bash
   code UserFunctions.cs
   ```

1. Find the comment **Create a function to submit a support ticket** and add the following code, which generates a ticket number and saves a support ticket as a text file.

   ```csharp
   // Create a function to submit a support ticket
   public static string SubmitSupportTicket(string emailAddress, string description)
   {
       string ticketNumber = Guid.NewGuid().ToString("N")[..6];
       string fileName = $"ticket-{ticketNumber}.txt";
       string filePath = Path.Combine(AppContext.BaseDirectory, fileName);
       string text = $"Support ticket: {ticketNumber}\nSubmitted by: {emailAddress}\nDescription:\n{description}";
       File.WriteAllText(filePath, text);

       var result = new { message = $"Support ticket {ticketNumber} submitted. The ticket file is saved as {fileName}" };
       return JsonSerializer.Serialize(result);
   }
   ```

1. Find the comment **Define function metadata for the agent** and add the following code, which defines the function metadata that the agent will use to understand when and how to call your function:

   ```csharp
   // Define function metadata for the agent
   public static FunctionToolDefinition GetSubmitSupportTicketTool()
   {
       return new FunctionToolDefinition(
           name: "submit_support_ticket",
           description: "Submit a technical support ticket with user email and issue description",
           parameters: BinaryData.FromObjectAsJson(new
           {
               type = "object",
               properties = new
               {
                   emailAddress = new
                   {
                       type = "string",
                       description = "The email address of the user submitting the ticket"
                   },
                   description = new
                   {
                       type = "string",
                       description = "A description of the technical issue"
                   }
               },
               required = new[] { "emailAddress", "description" }
           })
       );
   }
   ```

1. Save the file.

### Write code to implement an agent that can use your function

1. Enter the following command to begin editing the agent code.

   ```bash
   code Program.cs
   ```

   > **Tip**: As you add code to the code file, be sure to maintain the correct indentation.

1. Review the existing code, which retrieves the application configuration settings and sets up a loop in which the user can enter prompts for the agent. The rest of the file includes comments where you'll add the necessary code to implement your technical support agent.
1. Find the comment **Add references** and add the following code to import the namespaces you'll need to build an Azure AI agent that uses your function code as a tool:

   ```csharp
   // Add references
   using Azure.AI.Agents.Persistent;
   using Azure.Identity;
   using AgentLab02;
   ```

1. Find the comment **Connect to the Agent client** and add the following code to connect to the Azure AI project using the current Azure credentials.

   > **Tip**: Be careful to maintain the correct indentation level.

   ```csharp
   // Connect to the Agent client
   var agentsClient = new PersistentAgentsClient(
       projectConnectionString,
       new DefaultAzureCredential(new DefaultAzureCredentialOptions
       {
           ExcludeEnvironmentCredential = true,
           ExcludeManagedIdentityCredential = true
       })
   );
   ```

1. Find the comment **Define an agent that can use the custom functions** section, and add the following code to define the function tool, create an agent that can use it, and create a thread on which to run the chat session.

   ```csharp
   // Define an agent that can use the custom functions
   var functionTool = UserFunctions.GetSubmitSupportTicketTool();

   PersistentAgent agent = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: "support-agent-xxx",
       instructions: """
           You are a technical support agent.
           When a user has a technical issue, you get their email address and a description of the issue.
           Then you use those values to submit a support ticket using the function available to you.
           If a file is saved, tell the user the file name.
           """,
       tools: [functionTool]
   );

   PersistentAgentThread thread = await agentsClient.Threads.CreateThreadAsync();
   Console.WriteLine($"You're chatting with: {agent.Name} ({agent.Id})");
   ```

   > **Note**: To avoid naming conflicts with other students, use "support-agent-xxx" where `xxx` is the first three letters of your first name (e.g., "support-agent-ali" for Alice).

1. Find the comment **Send a prompt to the agent** and add the following code to add the user's prompt as a message and run the thread.

   ```csharp
   // Send a prompt to the agent
   PersistentThreadMessage message = await agentsClient.Messages.CreateMessageAsync(
       threadId: thread.Id,
       role: MessageRole.User,
       content: userPrompt
   );

   ThreadRun run = await agentsClient.Runs.CreateRunAsync(
       thread: thread,
       agent: agent
   );
   
   while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
   {
       if (run.Status == RunStatus.RequiresAction && run.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
       {
           var toolOutputs = new List<ToolOutput>();
           foreach (var toolCall in submitToolOutputsAction.ToolCalls)
           {
               if (toolCall is FunctionToolCall functionToolCall)
               {
                   if (functionToolCall.Name == "submit_support_ticket")
                   {
                       var args = JsonSerializer.Deserialize<Dictionary<string, string>>(functionToolCall.Arguments);
                       string emailAddress = args?["emailAddress"] ?? "";
                       string description = args?["description"] ?? "";
                       
                       string result = UserFunctions.SubmitSupportTicket(emailAddress, description);
                       toolOutputs.Add(new ToolOutput(functionToolCall.Id, result));
                   }
               }
           }
           
           run = await agentsClient.Runs.SubmitToolOutputsAsync(run, toolOutputs);
       }
       
       await Task.Delay(1000);
       run = await agentsClient.Runs.GetRunAsync(thread.Id, run.Id);
   }
   ```

1. Find the comment **Check the run status for failures** and add the following code to show any errors that occur.

   ```csharp
   // Check the run status for failures
   if (run.Status == RunStatus.Failed)
   {
       Console.WriteLine($"Run failed: {run.LastError?.Message}");
   }
   ```

1. Find the comment **Show the latest response from the agent** and add the following code to retrieve the messages from the completed thread and display the last one that was sent by the agent.

   ```csharp
   // Show the latest response from the agent
   var messages = agentsClient.Messages.GetMessagesAsync(thread.Id);
   await foreach (var msg in messages)
   {
       if (msg.Role == MessageRole.Agent)
       {
           foreach (var content in msg.ContentItems)
           {
               if (content is MessageTextContent textContent)
               {
                   Console.WriteLine($"Last Message: {textContent.Text}");
               }
           }
           break;
       }
   }
   ```

1. Find the comment **Get the conversation history** and add the following code to print out the messages from the conversation thread in chronological sequence:

   ```csharp
   // Get the conversation history
   Console.WriteLine("\nConversation Log:\n");
   var allMessages = agentsClient.Messages.GetMessagesAsync(
       threadId: thread.Id, 
       order: ListSortOrder.Ascending
   );
   
   await foreach (var msg in allMessages)
   {
       foreach (var content in msg.ContentItems)
       {
           if (content is MessageTextContent textContent)
           {
               Console.WriteLine($"{msg.Role}: {textContent.Text}\n");
           }
       }
   }
   ```

1. Find the comment **Clean up** and add the following code to delete the agent when no longer needed.

   ```csharp
   // Clean up
   await agentsClient.Administration.DeleteAgentAsync(agent.Id);
   Console.WriteLine("Deleted agent");
   ```

1. Review the code, using the comments to understand how it:

   - Defines a custom function with metadata
   - Creates an agent that uses the function as a tool
   - Runs a thread with a prompt message from the user
   - Handles function calling when the agent requires it
   - Checks the status of the run in case there's a failure
   - Retrieves the messages from the completed thread and displays the last one sent by the agent
   - Displays the conversation history
   - Deletes the agent when it's no longer required

1. Save the code file when you have finished. You can also close the code editor; though you may want to keep it open in case you need to make any edits to the code you added.

### Run the app

1. In the terminal, enter the following command to build the application:

   ```bash
   dotnet build
   ```

1. Then run the application:

   ```bash
   dotnet run
   ```

   The application runs using your Azure credentials to connect to your project and create and run the agent.

1. When prompted, enter a prompt such as:

   ```
   I have a technical problem
   ```

   > **Tip**: If the app fails because the rate limit is exceeded. Wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

1. View the response. The agent may ask for your email address and a description of the issue. You can use any email address (for example, `alex@contoso.com`) and any issue description (for example `my computer won't start`)

   When it has enough information, the agent should choose to use your function as required.

1. You can continue the conversation if you like. The thread is _stateful_, so it retains the conversation history - meaning that the agent has the full context for each response. Enter `quit` when you're done.
1. Review the conversation messages that were retrieved from the thread, and the tickets that were generated.
1. The tool should have saved support tickets in the app folder. You can use the `ls` command to check, and then use the `cat` command to view the file contents, like this:

   ```bash
   ls ticket-*.txt
   cat ticket-<ticket_num>.txt
   ```

   Replace `<ticket_num>` with the actual ticket number from the output.

## Summary

In this exercise, you used the Azure AI Agent Service SDK to create a custom function tool that an AI agent can use. The agent dynamically chose when to call your function based on the conversation context, enabling it to perform tasks beyond what the base model can do.
