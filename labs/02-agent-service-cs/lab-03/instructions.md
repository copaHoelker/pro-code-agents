---
lab:
    title: 'Develop a multi-agent solution with Microsoft Foundry'
    description: 'Learn to configure multiple agents to collaborate using Microsoft Foundry Agent Service'
---

# Develop a multi-agent solution

In this exercise, you'll create a project that orchestrates multiple AI agents using Microsoft Foundry Agent Service. You'll design an AI solution that assists with ticket triage. The connected agents will assess the ticket's priority, suggest a team assignment, and determine the level of effort required to complete the ticket. Let's get started!

> **Tip**: The code used in this exercise is based on the for Foundry SDK for C#. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

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

## Create an AI Agent client app

Now you're ready to create a client app that defines the agents and instructions. The code files are provided in the labfiles folder.

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

1. In the configuration file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Foundry portal) and ensure that the Model variable is set to **gpt-4o**.

1. After you've replaced the placeholders, save your changes and close the code editor.

### Create AI agents

Now you're ready to create the agents for your multi-agent solution! Let's get started!

1. Enter the following command to edit the **Program.cs** file:

   ```bash
   code Program.cs
   ```

1. Review the code in the file, noting that it loads configuration settings.

1. Find the comment **Add references** and add the following code to import the namespaces you'll need:

   ```csharp
   // Add references
   using Azure.AI.Agents.Persistent;
   using Azure.Identity;
   ```

1. Note that code to load the project endpoint and model name from your configuration has been provided.

1. Find the comment **Connect to the agents client**, and add the following code to create a PersistentAgentsClient connected to your project:

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

   Now you'll add code that uses the PersistentAgentsClient to create multiple agents, each with a specific role to play in processing a support ticket.

1. Find the comment **Create an agent to prioritize support tickets**, and enter the following code:

   ```csharp
   // Create an agent to prioritize support tickets
   string priorityAgentName = "priority_agent-xxx";
   string priorityAgentInstructions = """
   Assess how urgent a ticket is based on its description.

   Respond with one of the following levels:
   - High: User-facing or blocking issues
   - Medium: Time-sensitive but not breaking anything
   - Low: Cosmetic or non-urgent tasks

   Only output the urgency level and a very brief explanation.
   """;

   PersistentAgent priorityAgent = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: priorityAgentName,
       instructions: priorityAgentInstructions
   );
   ```

   > **Note**: To avoid naming conflicts with other students, use "priority_agent-xxx" where `xxx` is the first three letters of your first name (e.g., "priority_agent-ali" for Alice).

1. Find the comment **Create an agent to assign tickets to the appropriate team**, and enter the following code:

   ```csharp
   // Create an agent to assign tickets to the appropriate team
   string teamAgentName = "team_agent-xxx";
   string teamAgentInstructions = """
   Decide which team should own each ticket.

   Choose from the following teams:
   - Frontend
   - Backend
   - Infrastructure
   - Marketing

   Base your answer on the content of the ticket. Respond with the team name and a very brief explanation.
   """;

   PersistentAgent teamAgent = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: teamAgentName,
       instructions: teamAgentInstructions
   );
   ```

   > **Note**: Use the same suffix (first three letters of your first name) for this agent name.

1. Find the comment **Create an agent to estimate effort for a support ticket**, and enter the following code:

   ```csharp
   // Create an agent to estimate effort for a support ticket
   string effortAgentName = "effort_agent-xxx";
   string effortAgentInstructions = """
   Estimate how much work each ticket will require.

   Use the following scale:
   - Small: Can be completed in a day
   - Medium: 2-3 days of work
   - Large: Multi-day or cross-team effort

   Base your estimate on the complexity implied by the ticket. Respond with the effort level and a brief justification.
   """;

   PersistentAgent effortAgent = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: effortAgentName,
       instructions: effortAgentInstructions
   );
   ```

   > **Note**: Use the same suffix (first three letters of your first name) for this agent name.

   So far, you've created three agents; each of which has a specific role in triaging a support ticket. Now let's create AgentToolDefinition objects for each of these agents so they can be used by other agents.

1. Find the comment **Create connected agent tools for the support agents**, and enter the following code:

   ```csharp
   // Create connected agent tools for the support agents
   var priorityAgentTool = new AgentToolDefinition(
       agentId: priorityAgent.Id,
       name: priorityAgentName,
       description: "Assess the priority of a ticket"
   );

   var teamAgentTool = new AgentToolDefinition(
       agentId: teamAgent.Id,
       name: teamAgentName,
       description: "Determines which team should take the ticket"
   );

   var effortAgentTool = new AgentToolDefinition(
       agentId: effortAgent.Id,
       name: effortAgentName,
       description: "Determines the effort required to complete the ticket"
   );
   ```

   Now you're ready to create a primary agent that will coordinate the ticket triage process, using the connected agents as required.

1. Find the comment **Create an agent to triage support ticket processing by using connected agents**, and enter the following code:

   ```csharp
   // Create an agent to triage support ticket processing by using connected agents
   string triageAgentName = "triage_agent-xxx";
   string triageAgentInstructions = """
   Triage the given ticket. Use the connected tools to determine the ticket's priority,
   which team it should be assigned to, and how much effort it may take.
   """;

   PersistentAgent triageAgent = await agentsClient.Administration.CreateAgentAsync(
       model: model,
       name: triageAgentName,
       instructions: triageAgentInstructions,
       tools: [priorityAgentTool, teamAgentTool, effortAgentTool]
   );
   ```

   > **Note**: Use the same suffix (first three letters of your first name) for this agent name.

   Now that you have defined a primary agent, you can submit a prompt to it and have it use the other agents to triage a support issue.

1. Find the comment **Use the agents to triage a support issue**, and enter the following code:

   ```csharp
   // Use the agents to triage a support issue
   Console.WriteLine("Creating agent thread.");
   PersistentAgentThread thread = await agentsClient.Threads.CreateThreadAsync();

   // Create the ticket prompt
   Console.Write("\nWhat's the support problem you need to resolve?: ");
   string? prompt = Console.ReadLine();
   if (string.IsNullOrEmpty(prompt))
   {
       Console.WriteLine("No prompt provided. Exiting.");
       return;
   }

   // Send a prompt to the agent
   PersistentThreadMessage message = await agentsClient.Messages.CreateMessageAsync(
       threadId: thread.Id,
       role: MessageRole.User,
       content: prompt
   );

   // Run the thread using the primary agent
   Console.WriteLine("\nProcessing agent thread. Please wait.");
   ThreadRun run = await agentsClient.Runs.CreateRunAsync(
       thread: thread,
       agent: triageAgent
   );

   while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.RequiresAction)
   {
       await Task.Delay(1000);
       run = await agentsClient.Runs.GetRunAsync(thread.Id, run.Id);
   }

   if (run.Status == RunStatus.Failed)
   {
       Console.WriteLine($"Run failed: {run.LastError?.Message}");
   }

   // Fetch and display messages
   var messages = agentsClient.Messages.GetMessagesAsync(
       threadId: thread.Id, 
       order: ListSortOrder.Ascending
   );
   
   await foreach (var msg in messages)
   {
       foreach (var content in msg.ContentItems)
       {
           if (content is MessageTextContent textContent)
           {
               Console.WriteLine($"{msg.Role}:\n{textContent.Text}\n");
           }
       }
   }
   ```

1. Find the comment **Clean up**, and enter the following code to delete the agents when they are no longer required:

   ```csharp
   // Clean up
   Console.WriteLine("Cleaning up agents:");
   await agentsClient.Administration.DeleteAgentAsync(triageAgent.Id);
   Console.WriteLine("Deleted triage agent.");
   await agentsClient.Administration.DeleteAgentAsync(priorityAgent.Id);
   Console.WriteLine("Deleted priority agent.");
   await agentsClient.Administration.DeleteAgentAsync(teamAgent.Id);
   Console.WriteLine("Deleted team agent.");
   await agentsClient.Administration.DeleteAgentAsync(effortAgent.Id);
   Console.WriteLine("Deleted effort agent.");
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

1. Enter a prompt, such as `Users can't reset their password from the mobile app.`

   After the agents process the prompt, you should see some output similar to the following:

   ```output
   Creating agent thread.
   Processing agent thread. Please wait.

   MessageRole.User:
   Users can't reset their password from the mobile app.

   MessageRole.Agent:
   **Priority:** High
   **Explanation:** This is a user-facing issue that blocks users from performing a critical function (password reset), which can impact their ability to access the application.

   **Team:** Frontend
   **Explanation:** The issue is related to the mobile app interface, which falls under the Frontend team's responsibility.

   **Effort:** Medium
   **Justification:** Fixing password reset functionality typically involves both client-side changes (mobile app) and potentially backend authentication service verification, requiring 2-3 days of work.
   ```

   > **Tip**: The actual output may vary based on the specific ticket description and the model's response.

## Summary

In this exercise, you learned how to create a multi-agent solution using Microsoft Foundry Agent Service. You created multiple specialized agents and used connected agent tools to enable collaboration between them. This pattern allows you to build complex AI solutions by composing simpler, specialized agents.
