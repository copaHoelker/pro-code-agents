---
lab:
    title: 'Connect to remote agents with A2A protocol'
    description: 'Use the A2A protocol to collaborate with remote agents.'
---

# Connect to remote agents with A2A protocol

In this exercise, you'll use Azure AI Agent Service with the A2A (Agent-to-Agent) protocol to create simple remote agents that interact with one another. These agents will assist technical writers with preparing their developer blog posts. A title agent will generate a headline, and an outline agent will use the title to develop a concise outline for the article. Let's get started!

> **Tip**: The code used in this exercise is based on the Microsoft Foundry SDK for C# and the A2A .NET SDK. You can develop similar solutions using the SDKs for Python, JavaScript, and Java. Refer to [Microsoft Foundry SDK client libraries](https://learn.microsoft.com/azure/ai-foundry/how-to/develop/sdk-overview) for details.

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

## Create an A2A application

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

   The provided files include:

   ```output
   CSharp
   ├── TitleAgent/
   │   ├── TitleAgent.cs
   │   └── TitleAgent.csproj
   ├── OutlineAgent/
   │   ├── OutlineAgent.cs
   │   └── OutlineAgent.csproj
   ├── RoutingAgent/
   │   ├── RoutingAgent.cs
   │   └── RoutingAgent.csproj
   ├── Client/
   │   ├── Program.cs
   │   └── Client.csproj
   ├── appsettings.json
   └── A2ALab.sln
   ```

   Each agent folder contains the Azure AI agent code and an ASP.NET Core server to host the agent. The **routing agent** is responsible for discovering and communicating with the **title** and **outline** agents. The **Client** allows users to submit prompts to the routing agent.

### Configure the application settings

1. In the terminal, enter the following command to restore the NuGet packages for all projects:

   ```bash
   dotnet restore
   ```

1. Enter the following command to edit the configuration file that has been provided:

   ```bash
   code appsettings.json
   ```

   The file is opened in a code editor.

1. In the configuration file, replace the **your_project_endpoint** placeholder with the endpoint for your project (copied from the project **Overview** page in the Foundry portal) and ensure that the Model variable is set to **gpt-4o**.

1. After you've replaced the placeholder, save your changes and then close the code editor.

### Create a discoverable agent

In this task, you create the title agent that helps writers create trendy headlines for their articles. You also define the agent's skills and card required by the A2A protocol to make the agent discoverable.

> **Tip**: As you add code, be sure to maintain the correct indentation.

1. Enter the following command to edit the title agent's code file:

   ```bash
   code TitleAgent/TitleAgent.cs
   ```

1. Find the comment **Add references** and add the following using statements:

   ```csharp
   // Add references
   using A2A;
   using A2A.Server;
   using Azure.AI.Agents.Persistent;
   using Azure.Identity;
   ```

1. Find the comment **Create the agents client** and add the following code to connect to the Azure AI project:

   ```csharp
   // Create the agents client
   var agentsClient = new PersistentAgentsClient(
       config.ProjectConnectionString,
       new DefaultAzureCredential(new DefaultAzureCredentialOptions
       {
           ExcludeEnvironmentCredential = true,
           ExcludeManagedIdentityCredential = true
       })
   );
   ```

1. Find the comment **Create the title agent** and add the following code to create the agent:

   ```csharp
   // Create the title agent
   var agent = await agentsClient.Administration.CreateAgentAsync(
       model: config.Model,
       name: "title-agent-xxx",
       instructions: """
           You are a helpful writing assistant.
           Given a topic the user wants to write about, suggest a single clear and catchy blog post title.
           """
   );
   Console.WriteLine($"Title Agent created with ID: {agent.Id}");
   ```

   > **Note**: Replace "xxx" with the first three letters of your name to avoid conflicts.

1. Save the code file.

### Define agent skills for A2A discovery

1. In the same file, find the comment **Define agent skills** and add the following code:

   ```csharp
   // Define agent skills
   var skills = new List<AgentSkill>
   {
       new AgentSkill
       {
           Id = "generate_blog_title",
           Name = "Generate Blog Title",
           Description = "Generates a blog title based on a topic",
           Tags = new List<string> { "title" },
           Examples = new List<string> { "Can you give me a title for this article?" }
       }
   };
   ```

1. Find the comment **Create agent card** and add this code:

   ```csharp
   // Create agent card
   var agentCard = new AgentCard
   {
       Name = "AI Foundry Title Agent",
       Description = "An intelligent title generator agent powered by Foundry. I can help you generate catchy titles for your articles.",
       Url = $"http://{host}:{port}/",
       Version = "1.0.0",
       DefaultInputModes = new List<string> { "text" },
       DefaultOutputModes = new List<string> { "text" },
       Capabilities = new AgentCapabilities(),
       Skills = skills
   };
   ```

1. Save the code file when you have finished.

### Enable messages between the agents

In this task, you use the A2A protocol to enable the routing agent to send messages to the other agents.

1. Navigate to the RoutingAgent directory and edit the routing agent code:

   ```bash
   code RoutingAgent/RoutingAgent.cs
   ```

1. Find the comment **Retrieve the remote agent's A2A client** and add the following code:

   ```csharp
   // Retrieve the remote agent's A2A client
   var client = remoteAgentConnections[agentName];
   ```

1. Locate the comment **Construct the payload** and add the following code:

   ```csharp
   // Construct the payload
   var messageId = Guid.NewGuid().ToString();
   var payload = new MessageSendParams
   {
       Message = new Message
       {
           Role = "user",
           Parts = new List<Part> { new TextPart { Text = task } },
           MessageId = messageId
       }
   };
   ```

1. Find the comment **Send the message** and add the following code:

   ```csharp
   // Send the message to the remote agent client and await the response
   var request = new SendMessageRequest { Id = messageId, Params = payload };
   var response = await client.SendMessageAsync(request);
   ```

1. Save the code file when you have finished.

### Run the app

1. In the terminal, build all projects:

   ```bash
   dotnet build
   ```

1. Open multiple terminal windows (or use terminal tabs) and start each agent server:

   **Terminal 1 - Title Agent:**
   ```bash
   cd TitleAgent
   dotnet run
   ```

   **Terminal 2 - Outline Agent:**
   ```bash
   cd OutlineAgent
   dotnet run
   ```

   **Terminal 3 - Routing Agent:**
   ```bash
   cd RoutingAgent
   dotnet run
   ```

1. In a fourth terminal, run the client:

   ```bash
   cd Client
   dotnet run
   ```

1. Wait until the prompt for input appears, then enter a prompt such as:

   ```
   Create a title and outline for an article about React programming.
   ```

   After a few moments, you should see a response from the agents with the results.

1. Enter `quit` to exit the program.

1. Stop the agent servers by pressing **Ctrl+C** in each terminal.

## Summary

In this exercise, you used the Azure AI Agent Service SDK and the A2A .NET SDK to create a remote multi-agent solution. You created a discoverable A2A-compatible agent and set up a routing agent to access the agent's skills. You also implemented agent communication to process incoming A2A messages and manage tasks. Great work!
