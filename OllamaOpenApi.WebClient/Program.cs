using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OllamaOpenApi.WebClient;
using OllamaOpenApi.ServiceClient.Services;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:11435") });
builder.Services.AddScoped<IOllamaApiService, OllamaApiService>();

await builder.Build().RunAsync();
