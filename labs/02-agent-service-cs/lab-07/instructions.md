---
lab:
    title: 'Develop an AI agent with VS Code extension'
    description: 'Use the Microsoft Foundry VS Code extension to create an AI agent.'
---

# Develop an AI agent with VS Code extension

In this exercise, you'll use the Microsoft Foundry VS Code extension to create an agent that can use Model Context Protocol (MCP) server tools to access external data sources and APIs. The agent will be able to retrieve up-to-date information and interact with various services through MCP tools.

This exercise should take approximately **30** minutes to complete.

> **Note**: Some of the technologies used in this exercise are in preview or in active development. You may experience some unexpected behavior, warnings, or errors.

## Prerequisites

Before starting this exercise, ensure you have:

- Visual Studio Code installed
- An active Azure subscription

## Install the Microsoft Foundry VS Code extension

Let's start by installing and setting up the VS Code extension.

1. Open Visual Studio Code.

1. Select **Extensions** from the left pane (or press **Ctrl+Shift+X**).

1. In the search bar, type **Microsoft Foundry** and press Enter.

1. Select the **Microsoft Foundry** extension from Microsoft and click **Install**.

1. After installation is complete, verify the extension appears in the primary navigation bar on the left side of Visual Studio Code.

## Sign in to Azure and access the project

You'll use the existing Foundry project that has been pre-configured for this lab.

1. In the VS Code sidebar, select the **Microsoft Foundry** extension icon.

1. In the Resources view, select **Sign in to Azure...** and follow the authentication prompts.

   > **Note**: You won't see this option if you're already signed in.

1. After signing in, locate and select the **pro-code-agents-student** project in the Resources section.

The project has several pre-deployed models available for use, including **gpt-4o**, **gpt-4o-mini**, **gpt-4.1-mini**, **gpt-5-mini**, and **text-embedding-ada-002**.

1. Wait for the deployment to complete. Your deployed model will appear under the **Models** section in the Resources view.

## Create an AI agent with the designer view

Now you'll create an AI agent using the visual designer interface. Rather than writing code, you'll configure the agent's instructions, settings, and tools through the user interface.

1. In the Microsoft Foundry extension view, find the **Resources** section.

1. Select the **+** (plus) icon next to the **Declarative Agents** subsection to create a new AI Agent.

   ![Screenshot of an creating an agent in the Microsoft Foundry VS Code extension.](../Media/vs-code-new-agent.png)

1. Choose a location to save your agent files when prompted.

1. A **New Agent** tab will open to an "Agent Preferences" editor, along with a `.yaml` configuration file.

### Configure your agent in the designer

1. In the agent preferences, configure the following fields:

   - **Name**: Enter a descriptive name for your agent (e.g., "data-research-agent")
   - **Model**: Select your GPT-4o deployment from the dropdown
   - **Instructions**: Enter system instructions such as:
     ```
     You are an AI agent that helps users research information from various sources. Use the available tools to access up-to-date information and provide comprehensive responses based on external data sources.
     ```

1. Save the configuration by selecting **File > Save** from the VS Code menu bar.

## Add an MCP Server tool to your agent

You'll now add a Model Context Protocol (MCP) server tool that allows your agent to access external APIs and data sources.

1. In the **TOOL** section of the designer, select the **Add tool** button in the top-right corner.

![Screenshot of adding a tool to an agent in the Foundry VS Code extension.](../Media/vs-code-agent-tools.png)

1. From the dropdown menu, choose **MCP Server**.

1. Configure the MCP Server tool with the following information:

   - **Server URL**: Enter the URL of an MCP server (e.g., `https://gitmcp.io/Azure/azure-rest-api-specs`)
   - **Server Label**: Enter a unique identifier (e.g., "github_docs_server")

1. Leave the **Allowed tools** dropdown empty to allow all tools from the MCP server.

1. Select the **Create tool** button to add the tool to your agent.

## Deploy your agent to Microsoft Foundry

1. In the agent designer view, select the **Create Agent on Microsoft Foundry** button in the bottom-left corner.

1. Wait for the deployment to complete.

1. In the VS Code navbar, refresh the **Resources** view. Your deployed agent should now appear under the **Declarative Agents** subsection.

## Test your agent in the playground

1. Right-click on your deployed agent in the **Declarative Agents** subsection.

1. Select **Open Playground** from the context menu.

1. The Agents Playground will open in a new tab within VS Code.

1. Type a test prompt such as:

   ```output
   Can you help me find documentation about Azure Container Apps and provide an example of how to create one?
   ```

1. Send the message and observe the authentication and approval prompts for the MCP Server tool:

   - For this exercise, select **No Authentication** when prompted.
   - For the MCP Tools approval preference, you can select **Always approve**.

1. Review the agent's response and note how it uses the MCP server tool to retrieve external information.

1. Check the **Agent Annotations** section to see the sources of information used by the agent.

## Generate sample code for your agent

1. Right-click on your deployed agent and select **Open Code File**, or select the **Open Code File** button in the Agent Preferences page.

1. Choose your preferred SDK from the dropdown (Python, .NET, JavaScript, or Java).

1. Select your preferred programming language.

1. Choose your preferred authentication method.

1. Review the generated sample code that demonstrates how to interact with your agent programmatically.

You can use this code as a starting point for building applications that leverage your AI agent.

## View conversation history and threads

1. In the **Azure Resources** view, expand the **Threads** subsection to see conversations created during your agent interactions.

1. Select a thread to view the **Thread Details** page, which shows:

   - Individual messages in the conversation
   - Run information and execution details
   - Agent responses and tool usage

1. Select **View run info** to see detailed JSON information about each run.

## Summary

In this exercise, you used the Foundry VS Code extension to create an AI agent with MCP server tools. The agent can access external data sources and APIs through the Model Context Protocol, enabling it to provide up-to-date information and interact with various services. You also learned how to test the agent in the playground and generate sample code for programmatic interaction.
