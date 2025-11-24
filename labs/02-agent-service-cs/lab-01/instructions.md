---
lab:
    title: 'Develop an AI agent'
    description: 'Use the Azure AI Agent Service to develop an agent that uses built-in tools.'
---

# Develop an AI agent

In this exercise, you'll use Azure AI Agent Service to create a simple agent that analyzes data and creates charts. The agent can use the built-in _Code Interpreter_ tool to dynamically generate any code required to analyze data.

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

## Create an agent client app

Now you're ready to create a client app that uses an agent. The code files are provided in the labfiles folder.

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

   The provided files include application code, configuration settings, and data.

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

### Write code for an agent app

> **Tip**: As you add code, be sure to maintain the correct indentation. Use the comment indentation levels as a guide.

1. Enter the following command to edit the code file that has been provided:

   ```bash
   code Program.cs
   ```

1. Review the existing code, which retrieves the application configuration settings and loads data from _data.txt_ to be analyzed. The rest of the file includes comments where you'll add the necessary code to implement your data analysis agent.
1. Find the comment **Add references** and add the following code to import the namespaces you'll need to build an Azure AI agent that uses the built-in code interpreter tool:

   ```csharp
   // Add references
   using Azure.AI.Agents.Persistent;
   using Azure.Identity;
   ```

1. Find the comment **Connect to the Agent client** and add the following code to connect to the Azure AI project.

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

   The code connects to the Foundry project using the current Azure credentials.

1. Find the comment **Upload the data file and create a CodeInterpreterTool**, and add the following code to upload the data file to the project and create a CodeInterpreterTool that can access the data in it:

   ```csharp
   // Upload the data file and create a CodeInterpreterTool
   PersistentAgentFileInfo file = await agentsClient.Files.UploadFileAsync(
       filePath: filePath,
       purpose: PersistentAgentFilePurpose.Agents
   );
   Console.WriteLine($"Uploaded {file.Filename}");

   var codeInterpreterTool = new CodeInterpreterToolDefinition();
   ```

1. Find the comment **Define an agent that uses the CodeInterpreterTool** and add the following code to define an AI agent that analyzes data and can use the code interpreter tool you defined previously:

   ```csharp
   // Define an agent that uses the CodeInterpreterTool
   PersistentAgent agent = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: "data-agent-xxx",
       instructions: "You are an AI agent that analyzes the data in the file that has been uploaded. Use Python to calculate statistical metrics as necessary.",
       tools: [codeInterpreterTool],
       toolResources: new ToolResources
       {
           CodeInterpreter = new CodeInterpreterToolResource
           {
               FileIds = { file.Id }
           }
       }
   );
   Console.WriteLine($"Using agent: {agent.Name}");
   ```

   > **Note**: To avoid naming conflicts with other students, use "data-agent-xxx" where `xxx` is the first three letters of your first name (e.g., "data-agent-ali" for Alice).

1. Find the comment **Create a thread for the conversation** and add the following code to start a thread on which the chat session with the agent will run:

   ```csharp
   // Create a thread for the conversation
   PersistentAgentThread thread = await agentsClient.Threads.CreateThreadAsync();
   ```

1. Note that the next section of code sets up a loop for a user to enter a prompt, ending when the user enters "quit".

1. Find the comment **Send a prompt to the agent** and add the following code to add a user message to the prompt (along with the data from the file that was loaded previously), and then run thread with the agent.

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
       await Task.Delay(1000);
       run = await agentsClient.Runs.GetRunAsync(thread.Id, run.Id);
   }
   ```

1. Find the comment **Check the run status for failures** and add the following code to check for any errors.

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

1. Find the comment **Get the conversation history**, which is after the loop ends, and add the following code to print out the messages from the conversation thread in chronological sequence:

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

1. Find the comment **Clean up** and add the following code to delete the agent and file when no longer needed.

   ```csharp
   // Clean up
   await agentsClient.Administration.DeleteAgentAsync(agent.Id);
   await agentsClient.Files.DeleteFileAsync(file.Id);
   ```

1. Review the code, using the comments to understand how it:

   - Connects to the AI Foundry project.
   - Uploads the data file and creates a code interpreter tool that can access it.
   - Creates a new agent that uses the code interpreter tool and has explicit instructions to use Python as necessary for statistical analysis.
   - Runs a thread with a prompt message from the user along with the data to be analyzed.
   - Checks the status of the run in case there's a failure
   - Retrieves the messages from the completed thread and displays the last one sent by the agent.
   - Displays the conversation history
   - Deletes the agent and file when they're no longer required.

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

1. When prompted, view the data that the app has loaded from the _data.txt_ text file. Then enter a prompt such as:

   ```
   What's the category with the highest cost?
   ```

   > **Tip**: If the app fails because the rate limit is exceeded. Wait a few seconds and try again. If there is insufficient quota available in your subscription, the model may not be able to respond.

1. View the response. Then enter another prompt, this time requesting a visualization:

   ```
   Create a text-based bar chart showing cost by category
   ```

1. View the response. Then enter another prompt, this time requesting a statistical metric:

   ```
   What's the standard deviation of cost?
   ```

   View the response.

1. You can continue the conversation if you like. The thread is _stateful_, so it retains the conversation history - meaning that the agent has the full context for each response. Enter `quit` when you're done.
1. Review the conversation messages that were retrieved from the thread - which may include messages the agent generated to explain its steps when using the code interpreter tool.

## Summary

In this exercise, you used the Azure AI Agent Service SDK to create a client application that uses an AI agent. The agent can use the built-in Code Interpreter tool to run dynamic Python code to perform statistical analyses.
