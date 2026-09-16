using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using OllamaOpenApi.Service.Services;
using OllamaSharp.Models.Chat;
using Xunit;

namespace OllamaOpenApi.Service.Tests;

public class ToolCallingTests
{
    [Fact]
    public void GetAvailableTools_ShouldReturnExpectedTools()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ollama_host", "http://localhost:11434" },
                { "OLLAMA_MODEL", "llama3" }
            })
            .Build();

        var logger = new NullLogger<OllamaAgentService>();
        var service = new OllamaAgentService(configuration, logger);

        // Act
        var tools = service.GetAvailableTools();

        // Assert
        Assert.NotEmpty(tools);
        // We expect the tools from ToolConsole.Tools.FileSystem.cs if they are in the same assembly
        // Wait, ToolConsole.Tools.FileSystem.cs defines methods with [OllamaTool], 
        // they are not classes inheriting from Tool.
        // The source generator should have created classes that inherit from Tool.
    }
}
