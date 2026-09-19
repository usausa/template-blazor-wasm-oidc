using Microsoft.Extensions.Hosting.WindowsServices;

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

// System
builder.ConfigureSystem();

// Host
builder.ConfigureHost();

// Logging
builder.ConfigureLogging();

// Http
builder.ConfigureHttp();
// API
builder.ConfigureApi();
// Authentication
builder.ConfigureAuthentication();
// OpenApi
builder.ConfigureOpenApi();

// Health
builder.ConfigureHealth();
// Metrics
builder.ConfigureTelemetry();

// Components
builder.ConfigureComponents();

//--------------------------------------------------------------------------------
// Configure the HTTP request pipeline.
//--------------------------------------------------------------------------------
var app = builder.Build();

// Startup information
app.LogStartupInformation();

// Forwarded headers
app.UseForwardedHeaders();

// W3C log
app.UseW3CLog();

// Error handler
app.UseErrorHandler();

// HTTP log
app.UseHttpLog();

// Static files (Blazor WebAssembly assets)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Authentication
app.UseAuthentication();
app.UseAuthorization();

// End point
app.MapEndpoints();

// Initialize
await app.InitializeApplicationAsync();

// Run
await app.RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program;
