import os
import io
import sys
from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.agents import AgentsClient
from azure.ai.agents.models import (
    ListSortOrder,
    MessageTextContent,
    MessageInputContentBlock,
    MessageImageFileParam,
    MessageInputTextBlock,
    MessageInputImageFileBlock,
    FilePurpose,
    RunStatus,
)
from typing import List
from azure.monitor.opentelemetry import configure_azure_monitor
from opentelemetry.instrumentation.openai_v2 import OpenAIInstrumentor
from opentelemetry import trace

# Configure UTF-8 encoding for Windows console (fixes emoji display issues)
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

asset_file_path = os.path.join(os.path.dirname(__file__), "assets/soi.jpg")

# Clear the console to keep the output focused on the agent interaction
os.system('cls' if os.name == 'nt' else 'clear')

# Load environment variables from .env file
load_dotenv()
endpoint = os.getenv("PROJECT_ENDPOINT")
model = os.getenv("MODEL_DEPLOYMENT")

print(f"Using endpoint: {endpoint}")
print(f"Using model: {model}")

# Initialize the AI Project Client to get Application Insights connection string
project_client = AIProjectClient(
    credential=DefaultAzureCredential(),
    endpoint=endpoint,
)

# Get the Application Insights connection string for tracing
connection_string = project_client.telemetry.get_application_insights_connection_string()
print(f"Application Insights configured for tracing")

# Configure Azure Monitor with OpenTelemetry
configure_azure_monitor(connection_string=connection_string)

# Instrument OpenAI SDK to enable tracing
OpenAIInstrumentor().instrument()

# Get a tracer instance for custom spans
tracer = trace.get_tracer(__name__)

# Connect to the Azure AI Foundry project
agents_client = AgentsClient(
    endpoint=endpoint,
    credential=DefaultAzureCredential()
)

with agents_client:
    # Create agent with custom span
    with tracer.start_as_current_span("create_agent"):
        agent = agents_client.create_agent(
            model=model,
            name="my-agent",
            instructions="You are helpful agent",
        )
        print(f"Created agent, agent ID: {agent.id}")
        
        # Add custom attribute to span
        current_span = trace.get_current_span()
        current_span.set_attribute("agent.id", agent.id)
        current_span.set_attribute("agent.model", model)

    # Create thread with custom span
    with tracer.start_as_current_span("create_thread"):
        thread = agents_client.threads.create()
        print(f"Created thread, thread ID: {thread.id}")
        
        current_span = trace.get_current_span()
        current_span.set_attribute("thread.id", thread.id)

    # Upload file with custom span
    with tracer.start_as_current_span("upload_file"):
        image_file = agents_client.files.upload_and_poll(
            file_path=asset_file_path, 
            purpose=FilePurpose.AGENTS
        )
        print(f"Uploaded file, file ID: {image_file.id}")
        
        current_span = trace.get_current_span()
        current_span.set_attribute("file.id", image_file.id)
        current_span.set_attribute("file.path", asset_file_path)

    # Create message with image
    with tracer.start_as_current_span("create_message"):
        input_message = "Hello, what is in the image?"
        file_param = MessageImageFileParam(file_id=image_file.id, detail="high")
        content_blocks: List[MessageInputContentBlock] = [
            MessageInputTextBlock(text=input_message),
            MessageInputImageFileBlock(image_file=file_param),
        ]
        message = agents_client.messages.create(
            thread_id=thread.id, 
            role="user", 
            content=content_blocks
        )
        print(f"Created message, message ID: {message.id}")
        
        current_span = trace.get_current_span()
        current_span.set_attribute("message.id", message.id)
        current_span.set_attribute("message.content", input_message)

    # Run agent with custom span
    with tracer.start_as_current_span("run_agent"):
        run = agents_client.runs.create_and_process(
            thread_id=thread.id, 
            agent_id=agent.id
        )
        
        current_span = trace.get_current_span()
        current_span.set_attribute("run.id", run.id)
        current_span.set_attribute("run.status", run.status)
        
        if run.status != RunStatus.COMPLETED:
            print(f"The run did not succeed: {run.status=}.")
            current_span.set_attribute("run.success", False)
        else:
            current_span.set_attribute("run.success", True)

    # Clean up
    agents_client.delete_agent(agent.id)
    print("Deleted agent")

    # Retrieve and display messages
    with tracer.start_as_current_span("retrieve_messages"):
        messages = agents_client.messages.list(
            thread_id=thread.id, 
            order=ListSortOrder.ASCENDING
        )
        
        message_count = 0
        for msg in messages:
            message_count += 1
            last_part = msg.content[-1]
            if isinstance(last_part, MessageTextContent):
                print(f"{msg.role}: {last_part.text.value}")
        
        current_span = trace.get_current_span()
        current_span.set_attribute("messages.count", message_count)

print("\nTracing complete. View traces in Azure AI Foundry portal under Tracing section.")
