# Food Catalog API

## Why this project exists

The Food Catalog API is a .NET 9 minimal Web API that exposes CRUD endpoints for curated menu items. It demonstrates how to combine Entity Framework Core with Azure integrations such as Application Insights, Event Grid, Key Vault, and Microsoft Entra ID (via `Microsoft.Identity.Web`). The service ships with seeded data, Swagger UI for self-service exploration, and feature flags to toggle optional integrations.

## Architecture at a glance

- **Entry point (`Program.cs`)** – wires up configuration binding (`FoodConfig`), dependency injection, Entity Framework Core SQL Server provider, Swagger/OpenAPI, CORS, authentication/authorization, and optional Application Insights telemetry.
- **MCP Server Integration** – The API also hosts a Model Context Protocol (MCP) server. `Program.cs` registers `AddMcpServer().WithHttpTransport().WithToolsFromAssembly()` and maps MCP endpoints via `app.MapMcp()`. Tools are discovered via reflection.
- **Controllers**
  - `FoodController` – RESTful CRUD endpoints for `FoodItem` entities. Publishes placeholder events when `FeatureManagement.PublishEvents` is enabled.
  - `ConfigController` – diagnostic endpoints that expose the bound configuration and environment variables (development use only).
- **MCP Tools (`Tools/FoodTools.cs`)** – Exposes catalog operations (list, search, add, update stock, remove) as MCP tools callable by AI models/agents. Responses use `FoodItemCollection` for consistent schema.
- **Data access (`Database/FoodDBContext.cs`)** – EF Core `DbContext` that ensures the database exists, configures decimal precision, and seeds demo menu items.
- **Models (`Model/FoodItem.cs`, `Shared/Delivery.cs`)** – domain objects and helper classes.
- **Azure integrations (`AppInsights/*`, `EventGrid/*`)** – helpers for telemetry and Event Grid publishing driven by feature flags.
- **Configuration (`Config/FoodConfig.cs`, `appsettings*.json`)** – strongly typed options record Azure settings, feature toggles, logging, and connection strings.

## API surface

| Method   | Route               | Description                                                                |
| -------- | ------------------- | -------------------------------------------------------------------------- |
| `GET`    | `/food`             | Returns all food items. Requires `access_as_user` scope when auth enabled. |
| `GET`    | `/food/{id}`        | Returns a single item by identifier.                                       |
| `POST`   | `/food`             | Inserts a new item (expects `FoodItem` in body).                           |
| `PUT`    | `/food`             | Updates an existing item. Ensure `ID` is set.                              |
| `DELETE` | `/food/{id}`        | Removes an item.                                                           |
| `GET`    | `/config`           | Returns the bound configuration (development diagnostics).                 |
| `GET`    | `/config/getAllEnv` | Dumps environment variables (development diagnostics).                     |

All endpoints are attributed with `[ApiController]` conventions for automatic model validation.

## MCP Tool Surface

| Tool                                                                        | Description                                                    |
| --------------------------------------------------------------------------- | -------------------------------------------------------------- |
| `ListFood`                                                                  | Returns all food items as `FoodItemCollection`.                |
| `SearchFood(searchTerm)`                                                    | Finds items whose name, code or description contains the term. |
| `AddFood(name, code?, description?, price, inStock, minStock, pictureUrl?)` | Inserts a new catalog item and returns a status message.       |
| `UpdateStock(id, amount)`                                                   | Adjusts `InStock` by the specified delta (negative allowed).   |
| `RemoveFood(id)`                                                            | Deletes a catalog item by id and returns a status message.     |

All tools are annotated with `[McpServerTool]` and discovered automatically; no manual registration needed beyond the initial server setup in `Program.cs`.

### MCP Inspector Configuration

To inspect and invoke the Food MCP tools locally, use the provided `inspector.config.json`:

```json
{
  "mcpServers": {
    "food-mcp": {
      "type": "streamable-http",
      "url": "http://localhost:47003",
      "note": "Local Food MCP Server running on port 47003"
    }
  }
}
```

The fixed port is defined in `Properties/launchSettings.json` (`applicationUrl: http://localhost:47003`) to ensure consistent tool discovery across sessions.

## Authentication and authorization

- Toggle `App.AuthEnabled` to `true` to enforce Microsoft Entra ID authentication.
- `Program.cs` wires `AddMicrosoftIdentityWebApi` and applies a global authorization policy requiring authenticated users.
- `FoodController` checks the `access_as_user` scope via `HttpContext.VerifyUserHasAnyAcceptedScope`.
- When disabled, the API runs anonymously—intended for local debugging only.

## Observability & integrations

- **Application Insights** – If `UseApplicationInsights` is enabled and a connection string is present, telemetry is forwarded with a custom role name (`net-food-api`). `AILogger` simplifies custom event emission.
- **Azure Key Vault** – When `UseKeyVaultWithMI` is `true`, the app requests the `DefaultDatabase` secret via `DefaultAzureCredential`. Requires managed identity access to the vault.
- **Azure Event Grid** – `EventGridPublisher` assembles `CloudEvent<FoodItem>` payloads and posts them to the configured Event Grid topic. The sample controller currently writes to console; integrate the publisher and error handling as needed.
- **Health checks** – The flag exists but no endpoints are registered yet. Consider adding `builder.Services.AddHealthChecks()` and `app.MapHealthChecks()`.
