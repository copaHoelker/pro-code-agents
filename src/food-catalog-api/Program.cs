using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FoodApi;
using FoodApp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration Configuration = builder.Configuration;
builder.Services.AddSingleton<IConfiguration>(Configuration);
var cfg = Configuration.Get<FoodConfig>();

// App insights using Feature Flag
if (cfg.FeatureManagement.UseApplicationInsights)
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        if (!string.IsNullOrWhiteSpace(cfg.ApplicationInsights?.ConnectionString))
        {
            options.ConnectionString = cfg.ApplicationInsights.ConnectionString;
        }
    });
    builder.Services.AddSingleton<AILogger>();
}

//Database
builder.Services.AddDbContext<FoodDBContext>(options => options.UseSqlite("Data Source=foodcatalog.db"));

//Microsoft Identity auth
var az = Configuration.GetSection("Azure");

builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Cors
builder.Services.AddCors(o => o.AddPolicy("nocors", builder =>
{
    builder
        .SetIsOriginAllowed(host => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
}));

// MCP server registration: expose Food domain as tools to AI models
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
        options.Path = "";
    });
}

//Cors and Routing
app.UseCors("nocors");

app.MapControllers();

// Map MCP endpoints (JSON-RPC over HTTP transport)
app.MapMcp();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FoodDBContext>();
    db.Database.EnsureCreated();
}

app.Run();