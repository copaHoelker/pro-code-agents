# Lab Guide: Building a Remote MCP Server with Azure Functions (Python)

Welcome to this hands-on lab! In this guide, you'll learn how to build a Remote Model Context Protocol (MCP) Server using Azure Functions with Python. By the end of this lab, you'll have created three MCP tools and tested them locally.

[Remote MCP with Azure Functions (Python)](https://learn.microsoft.com/en-us/samples/azure-samples/remote-mcp-functions-python/remote-mcp-functions-python/)

![image](./_images/architecture-diagram.png)

## What You'll Build

You'll create an MCP server with three tools:

1. **Hello Tool** - A simple greeting function
2. **GetSnippet Tool** - Retrieves code snippets from blob storage
3. **SaveSnippet Tool** - Saves code snippets to blob storage

## Prerequisites

Before you begin, ensure you have the following installed:

- [Python](https://www.python.org/downloads/) version 3.11 or higher
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local?pivots=programming-language-python#install-the-azure-functions-core-tools) (version 4.0.7030 or later)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for running Azurite storage emulator)
- A code editor ([Visual Studio Code](https://code.visualstudio.com/) recommended)
- [Node.js](https://nodejs.org/) (for MCP Inspector)

### Verify Prerequisites

Open a terminal and verify your installations:

```bash
# Check Python version (should be 3.11 or later)
python --version

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
mkdir remote-mcp-functions-python
cd remote-mcp-functions-python
```

2. Create the Azure Functions project using the Azure Functions Core Tools:

```bash
func init mcp-tool --worker-runtime python --model V2
```

This creates a new Functions project in the `mcp-tool` directory with the following structure:

- `function_app.py` - Main application file where functions are defined
- `host.json` - Function host configuration
- `local.settings.json` - Local development settings
- `requirements.txt` - Python dependencies
- `.gitignore` - Git ignore file

3. Navigate to the mcp-tool directory:

```bash
cd mcp-tool
```

### Step 2: Update host.json for MCP Support

The MCP extension requires the experimental extension bundle. Update your `host.json` file:

```json
{
  "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  },
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle.Experimental",
    "version": "[4.*, 5.0.0)"
  }
}
```

**What's happening here?**

- The `extensionBundle` section specifies the experimental bundle that includes MCP support
- This bundle provides the `mcpToolTrigger` and blob bindings needed for our functions

### Step 3: Configure Local Settings

Update your `local.settings.json` file to use the local storage emulator:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true"
  }
}
```

### Step 4: Install Python Dependencies

Update your `requirements.txt` file:

```txt
# Do not include azure-functions-worker in this file
# The Python Worker is managed by the Azure Functions platform
# Manually managing azure-functions-worker may cause unexpected issues

azure-functions
```

Install the dependencies:

```bash
pip install -r requirements.txt
```

> **Note**: It's a best practice to create a virtual environment before installing dependencies. You can create one using:
>
> ```bash
> python -m venv .venv
> source .venv/bin/activate  # On Windows: .venv\Scripts\activate
> pip install -r requirements.txt
> ```

### Step 5: Start Azurite (Storage Emulator)

Azurite provides local blob, queue, and table storage emulation. You can run it using Docker, install it locally, or use the VS Code extension.

#### Option 1: Using Docker

In a **new terminal window**, start Azurite using Docker:

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 \
    mcr.microsoft.com/azure-storage/azurite
```

#### Option 2: Install Locally (Windows)

Install Azurite using npm:

```powershell
npm install -g azurite
```

Then start Azurite:

```powershell
azurite
```

#### Option 3: Install Locally (WSL/Linux/macOS)

Install Azurite using npm:

```bash
npm install -g azurite
```

Then start Azurite:

```bash
azurite
```

## Part 2: Create the Application Structure

Before implementing our functions, let's set up the basic structure of our `function_app.py` file.

### Step 1: Create function_app.py

Replace the contents of `function_app.py` with the following basic structure:

```python
import json
import logging

import azure.functions as func

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

# Constants for the Azure Blob Storage container, file, and blob path
_SNIPPET_NAME_PROPERTY_NAME = "snippetname"
_SNIPPET_PROPERTY_NAME = "snippet"
_BLOB_PATH = "snippets/{mcptoolargs." + _SNIPPET_NAME_PROPERTY_NAME + "}.json"


class ToolProperty:
    def __init__(self, property_name: str, property_type: str, description: str):
        self.propertyName = property_name
        self.propertyType = property_type
        self.description = description

    def to_dict(self):
        return {
            "propertyName": self.propertyName,
            "propertyType": self.propertyType,
            "description": self.description,
        }
```

**What's happening here?**

- We import the necessary modules: `json`, `logging`, and `azure.functions`
- We create a `FunctionApp` instance with function-level authentication
- We define constants for property names and the blob storage path
- The `ToolProperty` class helps us define the properties (parameters) for our MCP tools
- The `_BLOB_PATH` uses `{mcptoolargs.snippetname}` for dynamic binding based on tool parameters

## Part 3: Implement the Hello Tool

Now let's create our first MCP tool - a simple greeting function.

### Step 1: Add the Hello Tool Function

Add the following code to the end of your `function_app.py` file:

```python
@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="hello_mcp",
    description="Hello world.",
    toolProperties="[]",
)
def hello_mcp(context) -> None:
    """
    A simple function that returns a greeting message.

    Args:
        context: The trigger context (not used in this function).

    Returns:
        str: A greeting message.
    """
    return "Hello I am MCPTool!"
```

**What's happening here?**

- The `@app.generic_trigger` decorator registers this as an Azure Function with an MCP tool trigger
- The `type="mcpToolTrigger"` exposes it as an MCP tool
- The `toolName` parameter sets the name that MCP clients will use to invoke this tool
- The `description` parameter provides information about what the tool does
- The `toolProperties="[]"` indicates this tool takes no parameters (empty array)
- The function simply returns a greeting message

### Step 2: Test the Hello Tool

Start the Functions host to ensure everything is working:

```bash
func start
```

You should see output similar to:

```
Azure Functions Core Tools
Core Tools Version:       4.x.xxxx Commit hash: N/A +(...
Function Runtime Version: 4.x.x.xxxxx

Functions:

        hello_mcp: [GET,POST] http://localhost:7071/api/hello_mcp

For detailed output, run func with --verbose flag.
```

**Important**: The MCP endpoint is available at:

```
http://localhost:7071/runtime/webhooks/mcp/sse
```

This is different from the individual function endpoints listed above. The MCP endpoint provides a standardized Server-Sent Events (SSE) interface for MCP clients to discover and invoke all your tools.

Press `Ctrl+C` to stop the Functions host for now. We'll start it again after implementing all the tools.

## Part 4: Implement the Snippet Tools

Now let's implement the two snippet management tools that interact with blob storage.

### Step 1: Define Tool Properties

Add the following code after the `ToolProperty` class definition in your `function_app.py`:

```python
# Define the tool properties using the ToolProperty class
tool_properties_save_snippets_object = [
    ToolProperty(_SNIPPET_NAME_PROPERTY_NAME, "string", "The name of the snippet."),
    ToolProperty(_SNIPPET_PROPERTY_NAME, "string", "The content of the snippet."),
]

tool_properties_get_snippets_object = [
    ToolProperty(_SNIPPET_NAME_PROPERTY_NAME, "string", "The name of the snippet.")
]

# Convert the tool properties to JSON
tool_properties_save_snippets_json = json.dumps([prop.to_dict() for prop in tool_properties_save_snippets_object])
tool_properties_get_snippets_json = json.dumps([prop.to_dict() for prop in tool_properties_get_snippets_object])
```

**What's happening here?**

- We define the properties (parameters) for our snippet tools using the `ToolProperty` class
- For `save_snippet`, we need two properties: the snippet name and the snippet content
- For `get_snippet`, we only need the snippet name
- We convert these to JSON format, which is required by the MCP tool trigger

### Step 2: Add the GetSnippet Tool

Add the following code after the `hello_mcp` function:

```python
@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="get_snippet",
    description="Retrieve a snippet by name.",
    toolProperties=tool_properties_get_snippets_json,
)
@app.generic_input_binding(
    arg_name="file",
    type="blob",
    connection="AzureWebJobsStorage",
    path=_BLOB_PATH
)
def get_snippet(file: func.InputStream, context) -> str:
    """
    Retrieves a snippet by name from Azure Blob Storage.

    Args:
        file (func.InputStream): The input binding to read the snippet from Azure Blob Storage.
        context: The trigger context containing the input arguments.

    Returns:
        str: The content of the snippet or an error message.
    """
    snippet_content = file.read().decode("utf-8")
    logging.info(f"Retrieved snippet: {snippet_content}")
    return snippet_content
```

**What's happening here?**

- The `@app.generic_trigger` decorator with `type="mcpToolTrigger"` exposes this as an MCP tool
- The `toolProperties` parameter defines the input parameters (in this case, just `snippetname`)
- The `@app.generic_input_binding` decorator sets up automatic blob storage reading
  - `type="blob"` indicates this is a blob storage binding
  - `connection="AzureWebJobsStorage"` specifies which storage account to use
  - `path=_BLOB_PATH` uses our dynamic path with `{mcptoolargs.snippetname}` placeholder
- The function reads the blob content, logs it, and returns it

### Step 3: Add the SaveSnippet Tool

Add the following code after the `get_snippet` function:

```python
@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="save_snippet",
    description="Save a snippet with a name.",
    toolProperties=tool_properties_save_snippets_json,
)
@app.generic_output_binding(
    arg_name="file",
    type="blob",
    connection="AzureWebJobsStorage",
    path=_BLOB_PATH
)
def save_snippet(file: func.Out[str], context) -> str:
    content = json.loads(context)
    snippet_name_from_args = content["arguments"][_SNIPPET_NAME_PROPERTY_NAME]
    snippet_content_from_args = content["arguments"][_SNIPPET_PROPERTY_NAME]

    if not snippet_name_from_args:
        return "No snippet name provided"

    if not snippet_content_from_args:
        return "No snippet content provided"

    file.set(snippet_content_from_args)
    logging.info(f"Saved snippet: {snippet_content_from_args}")
    return f"Snippet '{snippet_content_from_args}' saved successfully"
```

**What's happening here?**

- The `@app.generic_trigger` decorator exposes this as an MCP tool with two properties
- The `@app.generic_output_binding` decorator sets up automatic blob storage writing
  - `type="blob"` indicates this is a blob storage binding for output
  - The path uses the same dynamic `_BLOB_PATH` to determine where to save the blob
- The function:
  1. Parses the context to extract the snippet name and content from the arguments
  2. Validates that both are provided
  3. Uses `file.set()` to write the content to blob storage
  4. Returns a success message

### Step 4: Understanding the Dynamic Blob Path

The pattern `{mcptoolargs.propertyname}` allows dynamic blob paths based on tool parameters:

```python
_BLOB_PATH = "snippets/{mcptoolargs.snippetname}.json"
```

This means:

- When `snippetname` is "test_snippet", the path becomes `snippets/test_snippet.json`
- When `snippetname` is "my_code", the path becomes `snippets/my_code.json`
- The `snippets` part is the blob container name
- The `.json` extension is added to the file name

## Part 5: Run the MCP Server Locally

Now let's run the complete MCP server locally and test it.

### Step 1: Start the Functions Host

From the `mcp-tool` directory, start the Functions host:

```bash
func start
```

You should see output showing all three functions:

```
Azure Functions Core Tools
Core Tools Version:       4.x.xxxx Commit hash: N/A +(...
Function Runtime Version: 4.x.x.xxxxx

Functions:

        hello_mcp: [GET,POST] http://localhost:7071/api/hello_mcp

        get_snippet: [GET,POST] http://localhost:7071/api/get_snippet

        save_snippet: [GET,POST] http://localhost:7071/api/save_snippet

For detailed output, run func with --verbose flag.
```

**Important**: The MCP endpoint is available at:

```
http://localhost:7071/runtime/webhooks/mcp/sse
```

Leave this terminal running - we'll use it throughout the testing process.

## Part 6: Test with MCP Inspector

MCP Inspector is a web-based tool for testing MCP servers. Let's use it to test our tools.

### Step 1: Install and Run MCP Inspector

In a **new terminal window** (keep the Functions host running), install and run MCP Inspector:

```bash
npx @modelcontextprotocol/inspector
```

You'll see output with a URL like:

```
MCP Inspector is running on http://0.0.0.0:5173
```

### Step 2: Connect to Your MCP Server

1. **Open the MCP Inspector**: Hold Ctrl (or Cmd on Mac) and click the URL to open it in your browser.

2. **Configure the connection**:

   - Set **Transport Type** to: `SSE` (Server-Sent Events)
   - Set **URL** to: `http://0.0.0.0:7071/runtime/webhooks/mcp/sse`
   - Click **Connect**

3. **Verify connection**: You should see a success message indicating that the connection was established.

### Step 3: Test the Hello Tool

1. Click **List Tools** in MCP Inspector
2. You should see three tools listed:

   - `hello_mcp`
   - `get_snippet`
   - `save_snippet`

3. Click on the **hello_mcp** tool
4. Click **Run Tool**
5. You should see the response: `"Hello I am MCPTool!"`

**Congratulations!** Your first MCP tool is working!

### Step 4: Test the SaveSnippet Tool

1. Click on the **save_snippet** tool
2. Fill in the parameters:
   - **snippetname**: `test_snippet`
   - **snippet**: `print("Hello from my snippet!")`
3. Click **Run Tool**
4. You should see a success message: `"Snippet 'print("Hello from my snippet!")' saved successfully"`

**What happened?**

- The snippet was saved to the local Azurite blob storage emulator
- It was saved in the `snippets` container as a file named `test_snippet.json`

### Step 5: Test the GetSnippet Tool

1. Click on the **get_snippet** tool
2. Fill in the parameter:
   - **snippetname**: `test_snippet`
3. Click **Run Tool**
4. You should see the snippet content: `print("Hello from my snippet!")`

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
5. Double-click on it to view the contents

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
- The `generic_trigger` with `type="mcpToolTrigger"` bridges these concepts, making Azure Functions discoverable as MCP tools

### Key Decorators and Bindings

1. **`@app.generic_trigger(type="mcpToolTrigger")`**: Exposes a function as an MCP tool with a name and description
2. **`@app.generic_input_binding(type="blob")`**: Automatically reads blob storage content into a parameter
3. **`@app.generic_output_binding(type="blob")`**: Automatically writes a value to blob storage

### Tool Properties

Tool properties define the parameters that an MCP tool accepts:

```python
class ToolProperty:
    def __init__(self, property_name: str, property_type: str, description: str):
        self.propertyName = property_name
        self.propertyType = property_type
        self.description = description
```

- `propertyName`: The parameter name
- `propertyType`: The data type (e.g., "string")
- `description`: Human-readable description of the parameter

### Dynamic Blob Paths

The pattern `{mcptoolargs.propertyname}` creates dynamic blob paths:

```python
_BLOB_PATH = "snippets/{mcptoolargs.snippetname}.json"
```

- This binds to the value of the `snippetname` parameter at runtime
- Different values create different blob paths automatically

### SSE (Server-Sent Events) Transport

The MCP endpoint uses SSE for communication:

- URL: `http://localhost:7071/runtime/webhooks/mcp/sse`
- SSE allows the server to push updates to clients
- It's a standard protocol that works over HTTP

## Troubleshooting

### Issue: Cannot connect to MCP server

**Symptoms**: MCP Inspector shows connection error

**Solutions**:

- Verify the Functions host is running (`func start`)
- Check the URL is correct: `http://0.0.0.0:7071/runtime/webhooks/mcp/sse`
- Ensure no firewall is blocking port 7071
- Try using `localhost` instead of `0.0.0.0` in the URL

### Issue: Blob storage errors

**Symptoms**: Error when saving or retrieving snippets

**Solutions**:

- Verify Azurite is running:
  ```bash
  docker ps | grep azurite
  ```
- Restart Azurite if needed
- Check `local.settings.json` has `"AzureWebJobsStorage": "UseDevelopmentStorage=true"`
- Ensure the experimental extension bundle is configured in `host.json`

### Issue: Import errors or module not found

**Symptoms**: `ModuleNotFoundError` when running `func start`

**Solutions**:

- Verify Python version: `python --version` (should be 3.11+)
- Reinstall dependencies:
  ```bash
  pip install -r requirements.txt
  ```
- If using a virtual environment, ensure it's activated

### Issue: MCP Inspector can't find tools

**Symptoms**: List Tools returns empty or doesn't work

**Solutions**:

- Verify the Functions host shows all three functions in the startup output
- Check the MCP endpoint URL is correct and includes `/sse`
- Ensure the experimental extension bundle is configured in `host.json`
- Try reconnecting in MCP Inspector

### Issue: Function runtime errors

**Symptoms**: Errors in the Functions host output

**Solutions**:

- Check the `host.json` file has the experimental extension bundle configured
- Verify all decorators are correctly formatted
- Check the Functions Core Tools version: `func --version` (should be 4.0.7030+)
- Review the logs for specific error messages

## Next Steps

Congratulations! You've successfully built a Remote MCP Server with Azure Functions using Python. Here are some suggestions for next steps:

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
- [Azure Functions Python Developer Guide](https://learn.microsoft.com/azure/azure-functions/functions-reference-python)

## Summary

In this lab, you learned how to:

✅ Set up an Azure Functions project with Python  
✅ Add MCP support using the experimental extension bundle  
✅ Create a simple Hello tool  
✅ Implement blob storage-backed tools for saving and retrieving snippets  
✅ Test your MCP server locally using MCP Inspector  
✅ Understand key MCP and Azure Functions concepts

You now have a solid foundation for building Remote MCP Servers with Azure Functions and Python!

## Appendix: Complete File Reference

### Directory Structure

```
remote-mcp-functions-python/
├── mcp-tool/
│   ├── function_app.py
│   ├── host.json
│   ├── local.settings.json
│   ├── requirements.txt
│   ├── .funcignore
│   └── .gitignore
└── README.md
```

### Complete function_app.py

```python
import json
import logging

import azure.functions as func

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

# Constants for the Azure Blob Storage container, file, and blob path
_SNIPPET_NAME_PROPERTY_NAME = "snippetname"
_SNIPPET_PROPERTY_NAME = "snippet"
_BLOB_PATH = "snippets/{mcptoolargs." + _SNIPPET_NAME_PROPERTY_NAME + "}.json"


class ToolProperty:
    def __init__(self, property_name: str, property_type: str, description: str):
        self.propertyName = property_name
        self.propertyType = property_type
        self.description = description

    def to_dict(self):
        return {
            "propertyName": self.propertyName,
            "propertyType": self.propertyType,
            "description": self.description,
        }


# Define the tool properties using the ToolProperty class
tool_properties_save_snippets_object = [
    ToolProperty(_SNIPPET_NAME_PROPERTY_NAME, "string", "The name of the snippet."),
    ToolProperty(_SNIPPET_PROPERTY_NAME, "string", "The content of the snippet."),
]

tool_properties_get_snippets_object = [
    ToolProperty(_SNIPPET_NAME_PROPERTY_NAME, "string", "The name of the snippet.")
]

# Convert the tool properties to JSON
tool_properties_save_snippets_json = json.dumps([prop.to_dict() for prop in tool_properties_save_snippets_object])
tool_properties_get_snippets_json = json.dumps([prop.to_dict() for prop in tool_properties_get_snippets_object])


@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="hello_mcp",
    description="Hello world.",
    toolProperties="[]",
)
def hello_mcp(context) -> None:
    """
    A simple function that returns a greeting message.

    Args:
        context: The trigger context (not used in this function).

    Returns:
        str: A greeting message.
    """
    return "Hello I am MCPTool!"


@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="get_snippet",
    description="Retrieve a snippet by name.",
    toolProperties=tool_properties_get_snippets_json,
)
@app.generic_input_binding(
    arg_name="file",
    type="blob",
    connection="AzureWebJobsStorage",
    path=_BLOB_PATH
)
def get_snippet(file: func.InputStream, context) -> str:
    """
    Retrieves a snippet by name from Azure Blob Storage.

    Args:
        file (func.InputStream): The input binding to read the snippet from Azure Blob Storage.
        context: The trigger context containing the input arguments.

    Returns:
        str: The content of the snippet or an error message.
    """
    snippet_content = file.read().decode("utf-8")
    logging.info(f"Retrieved snippet: {snippet_content}")
    return snippet_content


@app.generic_trigger(
    arg_name="context",
    type="mcpToolTrigger",
    toolName="save_snippet",
    description="Save a snippet with a name.",
    toolProperties=tool_properties_save_snippets_json,
)
@app.generic_output_binding(
    arg_name="file",
    type="blob",
    connection="AzureWebJobsStorage",
    path=_BLOB_PATH
)
def save_snippet(file: func.Out[str], context) -> str:
    content = json.loads(context)
    snippet_name_from_args = content["arguments"][_SNIPPET_NAME_PROPERTY_NAME]
    snippet_content_from_args = content["arguments"][_SNIPPET_PROPERTY_NAME]

    if not snippet_name_from_args:
        return "No snippet name provided"

    if not snippet_content_from_args:
        return "No snippet content provided"

    file.set(snippet_content_from_args)
    logging.info(f"Saved snippet: {snippet_content_from_args}")
    return f"Snippet '{snippet_content_from_args}' saved successfully"
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
      }
    }
  },
  "extensionBundle": {
    "id": "Microsoft.Azure.Functions.ExtensionBundle.Experimental",
    "version": "[4.*, 5.0.0)"
  }
}
```

### local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true"
  }
}
```

### requirements.txt

```txt
# Do not include azure-functions-worker in this file
# The Python Worker is managed by the Azure Functions platform
# Manually managing azure-functions-worker may cause unexpected issues

azure-functions
```

## Feedback

If you encounter any issues or have suggestions for improving this lab guide, please open an issue in the repository.

Happy coding! 🚀
