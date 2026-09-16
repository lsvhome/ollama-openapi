using OllamaOpenApi.Service.Data;
using OllamaOpenApi.Service.Services;
using Serilog;
using CommandLine;
using OllamaOpenApi.Service;

var builder = WebApplication.CreateBuilder(args);

// Parse command line arguments
var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
parserResult.WithParsed(options =>
{
    if (!string.IsNullOrEmpty(options.Model))
    {
        builder.Configuration["OLLAMA_MODEL"] = options.Model;
    }
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

app.Run();

/// <summary>
/// Marker class used by <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>
/// to locate the application entry point.
/// </summary>
public partial class Program { }
