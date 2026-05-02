using System.Reflection;
using ToDo.API;
using ToDo.Application;
using ToDo.Infrastructure;
using Microsoft.OpenApi;
using ToDo.API.Middlewares;
using ToDo.Persistence;
using ToDo.ServiceDefaults;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ToDo.API.Program>();

logger.LogInformation("Starting API");

var builder = WebApplication.CreateBuilder(args);

// Add Aspire Service Defaults (includes health checks, service discovery, etc.)
builder.AddServiceDefaults();

// Load and bind configuration
var configuration = builder.Configuration;

// Add Application services to the container.
builder.Services
    .AddApi()
    .AddApplication()
    .AddInfrastructure(configuration)
    .AddPersistence(configuration);

// Add Swagger
builder.Services
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "AI Assist API",
            Description = "ASP.NET Core Web API",
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        options.IncludeXmlComments(xmlPath);
    });

// Build the application 
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

// Redirect root to Swagger UI in development
    app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
}

// Add Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Redirect HTTP requests to HTTPS and Use HTTP Strict Transport Security Protocol in Production mode
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Enable routing, needed when using controllers
app.UseRouting();

// Use controllers as endpoints
app.MapControllers();

// // Map Aspire default endpoints (health checks, etc.)
app.MapDefaultEndpoints();

// Run the application
try
{
    await app.RunAsync();
}
catch (Exception e)
{
    logger.LogError("Application error: {Message}", e.Message);
}
finally
{
    logger.LogInformation("Shutting down API");
}

// Make Program class accessible to integration and E2E tests
namespace ToDo.API
{
    public partial class Program
    {
    }
}
