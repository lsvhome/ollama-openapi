using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;
using OllamaSharp.Models.Chat;
using Xunit;
using Moq;

namespace OllamaOpenApi.Service.Tests;

public class ToolCallingTests
{
    [Fact]
    public async Task GetAvailableTools_ShouldReturnExpectedTools()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ollama_host", "http://localhost:11434" },
                { "OLLAMA_MODEL", "llama3" },
                { "allow-local-tools", "true" }
            })
            .Build();

        var logger = new NullLogger<OllamaAgentService>();
        IExternalSource s = new ExternalSourceEmail(new NullLogger<ExternalSourceEmail>());

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddHttpClient();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var service = new OllamaAgentService(configuration, s, logger, httpClientFactory);

        // Act
        var tools = await service.GetAvailableToolsAsync();

        // Assert
        Assert.NotEmpty(tools);
        // We expect the tools from ToolConsole.Tools.FileSystem.cs if they are in the same assembly
        // Wait, ToolConsole.Tools.FileSystem.cs defines methods with [OllamaTool], 
        // they are not classes inheriting from Tool.
        // The source generator should have created classes that inherit from Tool.
    }
}
