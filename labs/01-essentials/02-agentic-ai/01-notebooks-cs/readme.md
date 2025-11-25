# C# Notebooks for Agentic AI

This directory contains C# implementations of the agentic AI notebooks using .NET Interactive and Azure AI Foundry SDK.

## Quick Start

Run the automated setup script:

```powershell
cd labs/01-essentials/02-agentic-ai/01-notebooks-cs
.\setup-notebooks.ps1
```

This script will:

- ✅ Verify .NET SDK installation (8.0+)
- ✅ Install .NET Interactive tools
- ✅ Check VS Code and install required extensions
- ✅ Verify Azure CLI (optional)
- ✅ Check environment configuration
- ✅ Register Jupyter kernels

### Script Options

```powershell
# Create a template .env file
.\setup-notebooks.ps1 -CreateEnvFile

# Skip VS Code checks (if using another editor)
.\setup-notebooks.ps1 -SkipVSCodeCheck

# Skip .NET checks (if already verified)
.\setup-notebooks.ps1 -SkipDotNetCheck
```

## Prerequisites

- .NET 8.0 or later
- Visual Studio Code with the following extensions:
  - [.NET Interactive Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
  - [Polyglot Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
- Azure AI Foundry project with deployed model
- Environment variables configured (see Configuration section)

## Notebooks

### 1. agentic-rag.ipynb

**Agentic RAG (Retrieval-Augmented Generation)**

Demonstrates building an agentic RAG system that combines retrieval-augmented generation with autonomous agent capabilities.

**Topics Covered:**

- Traditional RAG vs. Agentic RAG comparison
- Dynamic query planning
- Self-reflection and evaluation
- Query refinement and reformulation
- Multi-step reasoning
- Adaptive retrieval strategies

### 2. deep-reasoning.ipynb

**Deep Reasoning with Large Language Models**

Explores advanced reasoning techniques for complex problem-solving with LLMs.

**Topics Covered:**

- Chain-of-Thought (CoT) reasoning
- Few-shot CoT with examples
- Tree of Thoughts (multiple reasoning paths)
- Self-Consistency with majority voting
- Least-to-Most prompting (problem decomposition)
- Self-Verification and error correction
- Multi-Agent verification

### 3. react-framework.ipynb

**ReAct Framework: Reasoning and Acting**

Implements the ReAct framework that synergistically combines reasoning traces with task-specific actions.

**Topics Covered:**

- Basic ReAct pattern (Thought → Action → Observation)
- Tool usage and execution
- Enhanced ReAct with better prompting
- Multi-step reasoning with tools
- Error handling and recovery
- Self-reflection in ReAct loops

## Required NuGet Packages

All notebooks use the following NuGet packages (automatically installed when you run the first cell):

```xml
<PackageReference Include="Azure.AI.Projects" Version="1.0.0-beta.4" />
<PackageReference Include="Azure.Identity" Version="1.13.1" />
<PackageReference Include="DotNetEnv" Version="3.1.1" />
<PackageReference Include="Azure.AI.Inference" Version="1.0.0-beta.2" />
```

## Configuration

### Environment Variables

Create a `.env` file in the project root with the following variables:

```bash
PROJECT_ENDPOINT=https://your-project.api.azureml.ms
MODEL=gpt-4o
```

### Azure AI Foundry Setup

1. **Create an Azure AI Foundry project**

   - Go to [Azure AI Foundry](https://ai.azure.com)
   - Create a new project or use an existing one

2. **Deploy a model**

   - Deploy a chat model (e.g., GPT-4, GPT-4o, GPT-3.5-turbo)
   - Note the deployment name

3. **Get the project endpoint**

   - In your project, go to Settings
   - Copy the Project Connection String endpoint

4. **Set environment variables**
   - Update the `.env` file with your values
   - Ensure `MODEL` matches your deployment name

## Running the Notebooks

### Option 1: Automated Setup (Recommended)

1. **Run the setup script**

   ```powershell
   .\setup-notebooks.ps1 -CreateEnvFile
   ```

2. **Configure your credentials**

   - Edit the generated `.env` file
   - Add your `PROJECT_ENDPOINT` and `MODEL` values

3. **Open VS Code**

   ```powershell
   code .
   ```

4. **Open and run a notebook**
   - Select any `.ipynb` file
   - Choose ".NET Interactive" kernel
   - Click "Run All"

### Option 2: Manual Setup

1. **Open in VS Code**

   ```bash
   code labs/01-essentials/02-agentic-ai/01-notebooks-cs
   ```

2. **Open a notebook**

   - Click on any `.ipynb` file
   - VS Code should automatically detect it as a .NET Interactive notebook

3. **Select the kernel**

   - Click on the kernel selector in the top-right
   - Choose ".NET Interactive" or "C#"

4. **Run cells**
   - Click "Run All" or run cells individually
   - The first cell will install NuGet packages (may take a moment)

## Code Structure

### Common Patterns

All notebooks follow these C# patterns:

```csharp
// 1. Package installation and imports
#r "nuget: Azure.AI.Projects, 1.0.0-beta.4"
using Azure.AI.Projects;
using Azure.Identity;

// 2. Client initialization
var credential = new DefaultAzureCredential();
var projectClient = new AIProjectClient(new Uri(projectEndpoint), credential);
var chatClient = projectClient.GetChatClient();

// 3. Chat completion
var requestOptions = new ChatCompletionsOptions
{
    Temperature = 0.3f,
    MaxTokens = 300
};
requestOptions.Messages.Add(new ChatRequestUserMessage("Your prompt"));
var response = await chatClient.CompleteAsync(modelName, requestOptions);
```

### Helper Classes

Each notebook defines helper classes for structured data:

```csharp
// Document class for knowledge base
public class Document
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Category { get; set; }
}

// Result classes for agent responses
public class AgentResult
{
    public string Answer { get; set; }
    public int Iterations { get; set; }
    public List<TraceStep> Trace { get; set; }
    public string Status { get; set; }
}
```

## Key Differences from Python Notebooks

| Aspect                | Python              | C#                                    |
| --------------------- | ------------------- | ------------------------------------- |
| **Type System**       | Dynamic typing      | Static typing with generics           |
| **Collections**       | `list`, `dict`      | `List<T>`, `Dictionary<TKey, TValue>` |
| **Async**             | `async`/`await`     | `async Task`, `await`                 |
| **String Formatting** | f-strings           | String interpolation `$""`            |
| **SDK**               | `azure-ai-projects` | `Azure.AI.Projects`                   |
| **Null Handling**     | Optional types      | Nullable reference types              |

## Troubleshooting

### Common Issues

1. **"Could not load type" errors**

   - Restart the kernel: Click kernel icon → Restart
   - Clear outputs and run again

2. **NuGet package errors**

   - Check internet connection
   - Try running the package cell separately
   - Restart VS Code

3. **Authentication errors**

   - Ensure you're logged in to Azure CLI: `az login`
   - Check that `DefaultAzureCredential` can access your Azure resources
   - Verify your Azure subscription has access to AI Foundry

4. **Model not found errors**
   - Verify the model deployment name in `.env`
   - Check that the model is deployed in your AI Foundry project
   - Ensure the endpoint URL is correct

### Environment Setup

If `.env` file is not loading:

```csharp
// Alternative: Set environment variables directly
Environment.SetEnvironmentVariable("PROJECT_ENDPOINT", "https://...");
Environment.SetEnvironmentVariable("MODEL", "gpt-4o");
```

## Best Practices

1. **Run cells in order** - Notebooks are designed to be run sequentially
2. **Check outputs** - Verify each step before proceeding
3. **Modify examples** - Try changing prompts and parameters
4. **Monitor costs** - These notebooks make multiple API calls
5. **Use smaller models for testing** - GPT-3.5-turbo is faster and cheaper for development

## Performance Considerations

- **First run slower** - NuGet package installation takes time
- **API latency** - Network calls to Azure AI add latency
- **Token limits** - Complex examples may hit token limits (adjust `MaxTokens`)
- **Rate limiting** - Azure AI has rate limits; add delays if needed

## Additional Resources

- [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-studio/)
- [Azure.AI.Projects SDK](https://learn.microsoft.com/dotnet/api/azure.ai.projects)
- [.NET Interactive Notebooks](https://github.com/dotnet/interactive)
- [Polyglot Notebooks Documentation](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)

## Support

For issues or questions:

- Review the parent `readme.md` in `labs/01-essentials/02-agentic-ai/`
- Check the Python notebooks for comparison
- Consult Azure AI Foundry documentation
- Open an issue in the repository

## License

These notebooks are part of the Pro Code Agents training materials. See the repository root for license information.
