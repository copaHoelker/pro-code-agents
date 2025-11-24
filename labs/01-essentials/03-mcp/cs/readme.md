# Lab Guide: Building a Remote MCP Server with Azure Functions (.NET)

Welcome to this hands-on lab! In this guide, you'll learn how to build a Remote Model Context Protocol (MCP) Server using Azure Functions with .NET 10. By the end of this lab, you'll have created three MCP tools and tested them locally.

[Remote MCP with Azure Functions (.NET/C#)](https://learn.microsoft.com/en-us/samples/azure-samples/remote-mcp-functions-dotnet/remote-mcp-functions-dotnet/)

## What You'll Build

You'll create an MCP server with three tools:

1. **Hello Tool** - A simple greeting function
2. **GetSnippet Tool** - Retrieves code snippets from blob storage
3. **SaveSnippet Tool** - Saves code snippets to blob storage

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local?pivots=programming-language-csharp#install-the-azure-functions-core-tools) (version 4.0.7030 or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for running Azurite storage emulator)
- A code editor ([Visual Studio Code](https://code.visualstudio.com/) recommended)
- [Node.js](https://nodejs.org/) (for MCP Inspector)

### Verify Prerequisites

Open a terminal and verify your installations:

```bash
# Check .NET version (should be 10.0 or later)
dotnet --version

# Check Azure Functions Core Tools (should be 4.0.7030 or later)
func --version

# Check Docker
docker --version

# Check Node.js
node --version
```

## Part 1: Project Setup

### Step 1: Create a New Azure Functions Project

1. Create a new directory for your project:

```bash
mkdir remote-mcp-functions-dotnet
cd remote-mcp-functions-dotnet
```

2. Create the Azure Functions project using the Azure Functions Core Tools:

```bash
func init mcp-tool --worker-runtime dotnet-isolated --target-framework net10.0
```

This creates a new Functions project in the `mcp-tool` directory with the following structure:

- `Program.cs` - Application entry point
- `host.json` - Function host configuration
- `local.settings.json` - Local development settings
- `.gitignore` - Git ignore file

3. Navigate to the mcp-tool directory:

```bash
cd mcp-tool
```

### Step 2: Add Required NuGet Packages

The `func init` command for .NET 10 automatically includes most required packages. You only need to add the MCP and blob storage extensions:

```bash
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Mcp --version 1.0.0
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs --version 6.8.0
```

> **Note**: The following packages are already included by `func init` for .NET 10:
>
> - `Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore` (v2.1.0)
> - `Microsoft.ApplicationInsights.WorkerService` (v2.23.0)
> - `Microsoft.Azure.Functions.Worker.ApplicationInsights` (v2.50.0)

### Step 3: Configure Local Settings

Update your `local.settings.json` file to use the local storage emulator:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

### Step 4: Start Azurite (Storage Emulator)

Azurite provides local blob, queue, and table storage emulation. You can run it using Docker, install it locally, or use the VS Code extension.

#### Option 1: Using Docker

In a **new terminal window**, start Azurite using Docker:

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
    mcr.microsoft.com/azure-storage/azurite
```

#### Option 2: Install Locally (Windows)

Install Azurite using winget:

```powershell
winget install Microsoft.Azurite
```

Then start Azurite:

```powershell
azurite
```

#### Option 3: Install Locally (WSL/Linux)

Install Azurite using npm:

```bash
npm install -g azurite
```

Then start Azurite:

```bash
azurite
```

## Part 2: Create the Tool Information Class

Before implementing our functions, let's create a centralized class for tool metadata.

### Step 1: Create ToolsInformation.cs

Create a new file `ToolsInformation.cs` in the `mcp-tool` directory:

```csharp
namespace FunctionsSnippetTool;

internal sealed class ToolsInformation
{
    // Hello Tool
    public const string HelloToolName = "hello";
    public const string HelloToolDescription =
        "Simple hello world MCP Tool that responds with a hello message.";

    // GetSnippet Tool
    public const string GetSnippetToolName = "get_snippets";
    public const string GetSnippetToolDescription =
        "Gets code snippets from your snippet collection.";

    // SaveSnippet Tool
    public const string SaveSnippetToolName = "save_snippet";
    public const string SaveSnippetToolDescription =
        "Saves a code snippet into your snippet collection.";

    // Property definitions
    public const string SnippetNamePropertyName = "snippetname";
    public const string SnippetPropertyName = "snippet";
    public const string SnippetNamePropertyDescription = "The name of the snippet.";
    public const string SnippetPropertyDescription = "The code snippet.";
    public const string PropertyType = "string";
}
```

**What's happening here?**

- We define constants for all tool names, descriptions, and property metadata
- This centralization makes it easy to maintain consistency across the application
- The `internal sealed` modifier ensures this class is only used within the assembly

## Part 3: Implement the Hello Tool

Now let's create our first MCP tool - a simple greeting function.

### Step 1: Create HelloTool.cs

Create a new file `HelloTool.cs` in the `mcp-tool` directory:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using static FunctionsSnippetTool.ToolsInformation;

namespace FunctionsSnippetTool;

public class HelloTool(ILogger<HelloTool> logger)
{
    [Function(nameof(SayHello))]
    public string SayHello(
        [McpToolTrigger(HelloToolName, HelloToolDescription)] ToolInvocationContext context
    )
    {
        logger.LogInformation("Saying hello");
        return "Hello I am MCP Tool!";
    }
}
```

**What's happening here?**

- The `HelloTool` class uses **primary constructor syntax** (C# 12 feature) to inject the logger
- The `[Function]` attribute registers this as an Azure Function
- The `[McpToolTrigger]` attribute exposes it as an MCP tool with the specified name and description
- The `ToolInvocationContext` parameter provides context about the MCP tool invocation
- The method returns a simple string message

### Step 2: Build and Test

Build the project to ensure there are no errors:

```bash
dotnet build
```

You should see output indicating a successful build.

## Part 4: Implement the Snippet Tools

Now let's implement the two snippet management tools that interact with blob storage.

### Step 1: Create SnippetsTool.cs

Create a new file `SnippetsTool.cs` in the `mcp-tool` directory:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using static FunctionsSnippetTool.ToolsInformation;

namespace FunctionsSnippetTool;

public class SnippetsTool()
{
    private const string BlobPath = "snippets/{mcptoolargs." + SnippetNamePropertyName + "}.json";

    [Function(nameof(GetSnippet))]
    public object GetSnippet(
        [McpToolTrigger(GetSnippetToolName, GetSnippetToolDescription)]
            ToolInvocationContext context,
        [BlobInput(BlobPath)] string snippetContent
    )
    {
        return snippetContent;
    }

    [Function(nameof(SaveSnippet))]
    [BlobOutput(BlobPath)]
    public string SaveSnippet(
        [McpToolTrigger(SaveSnippetToolName, SaveSnippetToolDescription)]
            ToolInvocationContext context,
        [McpToolProperty(SnippetNamePropertyName, SnippetNamePropertyDescription, true)]
            string name,
        [McpToolProperty(SnippetPropertyName, SnippetPropertyDescription, true)]
            string snippet
    )
    {
        return snippet;
    }
}
```

**What's happening here?**

**GetSnippet Function:**

- Uses `[McpToolTrigger]` to expose as an MCP tool
- Uses `[BlobInput]` binding to automatically read from blob storage
- The blob path uses `{mcptoolargs.snippetname}` to dynamically bind to the snippet name parameter
- Returns the blob content as an object

**SaveSnippet Function:**

- Uses `[McpToolTrigger]` to expose as an MCP tool
- Uses `[BlobOutput]` binding to automatically write to blob storage
- The `[McpToolProperty]` attributes define the input parameters for the tool:
  - `name` - The snippet name (required)
  - `snippet` - The code snippet content (required)
- Returns the snippet content, which is automatically saved to blob storage

**Understanding the BlobPath:**

- `snippets/` - The container name
- `{mcptoolargs.snippetname}` - Dynamic binding that uses the value of the `snippetname` parameter
- `.json` - File extension for the saved snippet

### Step 2: Update Program.cs

Replace the contents of `Program.cs` with the following:

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static FunctionsSnippetTool.ToolsInformation;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Demonstrate how you can define tool properties without requiring
// input bindings:
builder
    .ConfigureMcpTool(GetSnippetToolName)
    .WithProperty(SnippetNamePropertyName, PropertyType, SnippetNamePropertyDescription, required: true);

builder.Build().Run();
```

**What's happening here?**

- We create a Functions application using the builder pattern
- We configure the Functions web application middleware
- We add Application Insights for telemetry (useful for production scenarios)
- We configure the `get_snippets` tool to have a `snippetname` property
  - This is an alternative way to define properties without using `[McpToolProperty]` attributes
  - It demonstrates the flexibility of the MCP SDK

### Step 3: Build the Complete Project

Build the project to ensure all components work together:

```bash
dotnet build
```

You should see a successful build with no errors.

## Part 5: Run the MCP Server Locally

Now let's run the MCP server locally and test it.

### Step 1: Start the Functions Host

From the `mcp-tool` directory, start the Functions host:

```bash
func start
```

You should see output similar to:

```
Azure Functions Core Tools
Core Tools Version:       4.x.xxxx Commit hash: N/A +(...
Function Runtime Version: 4.x.x.xxxxx

Functions:

        GetSnippet: [GET,POST] http://localhost:7071/api/GetSnippet

        SayHello: [GET,POST] http://localhost:7071/api/SayHello

        SaveSnippet: [GET,POST] http://localhost:7071/api/SaveSnippet

For detailed output, run func with --verbose flag.
```

**Important**: The MCP endpoint is available at:

```
http://localhost:7071/runtime/webhooks/mcp
```

This is different from the individual function endpoints listed above. The MCP endpoint provides a standardized way for MCP clients to discover and invoke all your tools.

## Part 6: Test with MCP Inspector

MCP Inspector is a web-based tool for testing MCP servers. Let's use it to test our tools.

### Step 1: Install and Run MCP Inspector

In a **new terminal window** (keep the Functions host running), install and run MCP Inspector:

```bash
npx @modelcontextprotocol/inspector node build/index.js
```

You'll see output with a URL like:

```
MCP Inspector is running on http://0.0.0.0:5173
```

### Step 2: Connect to Your MCP Server

1. **Open the MCP Inspector**: Hold Ctrl (or Cmd on Mac) and click the URL to open it in your browser.

2. **Configure the connection**:

   - Set **Transport Type** to: `Streamable HTTP`
   - Set **URL** to: `http://0.0.0.0:7071/runtime/webhooks/mcp`
   - Click **Connect**

3. **Verify connection**: You should see a success message indicating that the connection was established.

### Step 3: Test the Hello Tool

1. Click **List Tools** in MCP Inspector
2. You should see three tools listed:

   - `hello`
   - `get_snippets`
   - `save_snippet`

3. Click on the **hello** tool
4. Click **Run Tool**
5. You should see the response: `"Hello I am MCP Tool!"`

**Congratulations!** Your first MCP tool is working!

### Step 4: Test the SaveSnippet Tool

1. Click on the **save_snippet** tool
2. Fill in the parameters:
   - **snippetname**: `test_snippet`
   - **snippet**: `console.log("Hello from my snippet!");`
3. Click **Run Tool**
4. You should see the snippet content returned in the response

**What happened?**

- The snippet was saved to the local Azurite blob storage emulator
- It was saved in the `snippets` container as a file named `test_snippet.json`

### Step 5: Test the GetSnippet Tool

1. Click on the **get_snippets** tool
2. Fill in the parameter:
   - **snippetname**: `test_snippet`
3. Click **Run Tool**
4. You should see the snippet content: `console.log("Hello from my snippet!");`

**What happened?**

- The tool retrieved the snippet from blob storage
- It used the `snippetname` parameter to locate the correct blob

### Step 6: Verify Blob Storage (Optional)

You can verify that snippets are being stored in Azurite using Azure Storage Explorer or the Azure CLI.

**Using Azure Storage Explorer:**

1. Open Azure Storage Explorer
2. Connect to Local Storage Emulator
3. Navigate to **Blob Containers** → **snippets**
4. You should see `test_snippet.json`

**Using Azure CLI:**

```bash
# List blobs in the snippets container
az storage blob list --container-name snippets \
  --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
```

## Part 7: Understanding the Code

Let's review what you've built and understand the key concepts.

### MCP Tools vs Azure Functions

- **Azure Functions**: Individual serverless functions that can be triggered by HTTP, timers, queues, etc.
- **MCP Tools**: A standardized way to expose functions as tools that AI models can discover and invoke
- The `[McpToolTrigger]` attribute bridges these concepts, making Azure Functions discoverable as MCP tools

### Key Attributes

1. **`[Function]`**: Registers a method as an Azure Function
2. **`[McpToolTrigger]`**: Exposes the function as an MCP tool with a name and description
3. **`[McpToolProperty]`**: Defines input parameters for the tool
4. **`[BlobInput]`**: Automatically binds blob storage content to a parameter
5. **`[BlobOutput]`**: Automatically writes the return value to blob storage

### Dynamic Blob Paths

The pattern `{mcptoolargs.propertyname}` allows you to create dynamic blob paths based on tool parameters:

```csharp
private const string BlobPath = "snippets/{mcptoolargs.snippetname}.json";
```

This means:

- When `snippetname` is "test_snippet", the path becomes `snippets/test_snippet.json`
- When `snippetname` is "my_code", the path becomes `snippets/my_code.json`

### Dependency Injection

The `HelloTool` class uses dependency injection to receive an `ILogger`:

```csharp
public class HelloTool(ILogger<HelloTool> logger)
```

This is a modern C# primary constructor pattern, equivalent to:

```csharp
public class HelloTool
{
    private readonly ILogger<HelloTool> logger;

    public HelloTool(ILogger<HelloTool> logger)
    {
        this.logger = logger;
    }
}
```

## Troubleshooting

### Issue: Cannot connect to MCP server

**Symptoms**: MCP Inspector shows connection error

**Solutions**:

- Verify the Functions host is running (`func start`)
- Check the URL is correct: `http://0.0.0.0:7071/runtime/webhooks/mcp`
- Ensure no firewall is blocking port 7071

### Issue: Blob storage errors

**Symptoms**: Error when saving or retrieving snippets

**Solutions**:

- Verify Azurite is running:
  ```bash
  docker ps | grep azurite
  ```
- Restart Azurite if needed
- Check `local.settings.json` has `"AzureWebJobsStorage": "UseDevelopmentStorage=true"`

### Issue: Build errors

**Symptoms**: `dotnet build` fails

**Solutions**:

- Verify .NET 10 SDK is installed: `dotnet --version`
- Clean and rebuild:
  ```bash
  dotnet clean
  dotnet build
  ```
- Ensure all NuGet packages are restored:
  ```bash
  dotnet restore
  ```

### Issue: MCP Inspector can't find tools

**Symptoms**: List Tools returns empty or doesn't work

**Solutions**:

- Verify the Functions host shows all three functions in the startup output
- Check the MCP endpoint URL is correct
- Try reconnecting in MCP Inspector

## Next Steps

Congratulations! You've successfully built a Remote MCP Server with Azure Functions. Here are some suggestions for next steps:

### Enhance Your MCP Server

1. **Add more tools**: Create additional MCP tools for different operations
2. **Add validation**: Implement input validation for the snippet tools
3. **Add error handling**: Improve error messages and handling
4. **Add logging**: Use structured logging for better diagnostics

### Deploy to Azure

Follow the deployment guide in the main README.md to deploy your MCP server to Azure:

```bash
# Install Azure Developer CLI if not already installed
# Then deploy
azd up
```

### Connect with AI Clients

Test your MCP server with AI clients like:

- GitHub Copilot in VS Code
- Claude Desktop
- Other MCP-compatible clients

### Learn More

- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [Azure Functions MCP Extension](https://learn.microsoft.com/azure/azure-functions/functions-bindings-mcp)

## Summary

In this lab, you learned how to:

✅ Set up an Azure Functions project with .NET 10  
✅ Add MCP support using the MCP extension  
✅ Create a simple Hello tool  
✅ Implement blob storage-backed tools for saving and retrieving snippets  
✅ Test your MCP server locally using MCP Inspector  
✅ Understand key MCP and Azure Functions concepts

You now have a solid foundation for building Remote MCP Servers with Azure Functions!

## Appendix: Complete File Reference

### Directory Structure

```
remote-mcp-functions-dotnet/
├── mcp-tool/
│   ├── HelloTool.cs
│   ├── SnippetsTool.cs
│   ├── ToolsInformation.cs
│   ├── Program.cs
│   ├── host.json
│   ├── local.settings.json
│   ├── FunctionsMcpTool.csproj
│   └── Properties/
└── README.md
```

### Sample .csproj File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.23.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.50.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.ApplicationInsights" Version="2.50.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore" Version="2.1.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Mcp" Version="1.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Storage.Blobs" Version="6.8.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.6" />
  </ItemGroup>
  <ItemGroup>
    <None Update="host.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="local.settings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </None>
  </ItemGroup>
  <ItemGroup>
    <Using Include="System.Threading.ExecutionContext" Alias="ExecutionContext" />
  </ItemGroup>
</Project>
```

### host.json

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      },
      "enableLiveMetricsFilters": true
    }
  }
}
```

## Feedback

If you encounter any issues or have suggestions for improving this lab guide, please open an issue in the repository.

Happy coding! 🚀
