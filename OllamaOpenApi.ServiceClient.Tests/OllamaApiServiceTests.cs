using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using OllamaOpenApi.Dto;
using OllamaOpenApi.ServiceClient.Services;
using Xunit;

namespace OllamaOpenApi.ServiceClient.Tests;

public class OllamaApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<OllamaApiService>> _loggerMock;
    private readonly OllamaApiService _service;

    public OllamaApiServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<OllamaApiService>>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _service = new OllamaApiService(_httpClient, _loggerMock.Object);
    }

    private void SetupRequest(string method, string url, object? responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod(method) && req.RequestUri!.ToString().Contains(url)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = responseContent is string s 
                    ? new StringContent(s) 
                    : responseContent != null 
                        ? JsonContent.Create(responseContent) 
                        : new StringContent(string.Empty)
            });
    }

    [Fact]
    public async Task GetModelsAsync_ReturnsModels()
    {
        // Arrange
        var models = new List<ModelDto> { new ModelDto { Name = "Model1" } };
        SetupRequest("GET", "api/models/list", models);

        // Act
        var result = await _service.GetModelsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Model1", result[0].Name);
    }

    [Fact]
    public async Task GetDefaultModelAsync_ReturnsModel()
    {
        // Arrange
        var model = new ModelDto { Name = "DefaultModel" };
        SetupRequest("GET", "api/models/default", model);

        // Act
        var result = await _service.GetDefaultModelAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DefaultModel", result!.Name);
    }

    [Fact]
    public async Task GetDefaultModelAsync_ReturnsNullOnNotFound()
    {
        // Arrange
        SetupRequest("GET", "api/models/default", null, HttpStatusCode.NotFound);

        // Act
        var result = await _service.GetDefaultModelAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetDefaultModelAsync_SendsPutRequest()
    {
        // Arrange
        var request = new SetModelRequest("NewDefault");
        SetupRequest("PUT", "api/models/default", null);

        // Act
        await _service.SetDefaultModelAsync(request);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("PUT") && req.RequestUri!.ToString().Contains("api/models/default")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetOllamaStatusAsync_ReturnsStatus()
    {
        // Arrange
        var models = new List<OllamaStatusDto> { new OllamaStatusDto("Status1", null, 0, null, null, 0) };
        var response = new OllamaStatusResponseDto(models);
        SetupRequest("GET", "api/ollama/ps", response);

        // Act
        var result = await _service.GetOllamaStatusAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Status1", result[0].Name);
    }

    [Fact]
    public async Task PauseExecutionListAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/Processing/pause", null);

        // Act
        await _service.PauseExecutionListAsync();

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/Processing/pause")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ResumeExecutionListAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/Processing/resume", null);

        // Act
        await _service.ResumeExecutionListAsync();

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/Processing/resume")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task InterruptExecutionListAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/Processing/interrupt", null);

        // Act
        await _service.InterruptExecutionListAsync();

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/Processing/interrupt")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetActiveExecutionSessionAsync_ReturnsSession()
    {
        // Arrange
        var session = new SessionDto { Id = Guid.NewGuid(), Title = "Active" };
        SetupRequest("GET", "api/Processing/activeSession", session);

        // Act
        var result = await _service.GetActiveExecutionSessionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Active", result!.Title);
    }

    [Fact]
    public async Task GetExecutionListStatusAsync_ReturnsStatusString()
    {
        // Arrange
        var response = new ExecutionListStatusResponseDto(ExecutionListStatus.InProgress);
        SetupRequest("GET", "api/Processing/status", response);

        // Act
        var result = await _service.GetExecutionListStatusAsync();

        // Assert
        Assert.Equal("InProgress", result);
    }

    [Fact]
    public async Task LoadModelAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/ollama/load/test-model", null);

        // Act
        await _service.LoadModelAsync("test-model");

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/ollama/load/test-model")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UnloadModelAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/ollama/unload/test-model", null);

        // Act
        await _service.UnloadModelAsync("test-model");

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/ollama/unload/test-model")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ReloadModelAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/ollama/reload/test-model", null);

        // Act
        await _service.ReloadModelAsync("test-model");

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/ollama/reload/test-model")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsSessions()
    {
        // Arrange
        var sessions = new List<SessionDto> { new SessionDto { Id = Guid.NewGuid(), Title = "S1" } };
        SetupRequest("GET", "api/sessions/list", sessions);

        // Act
        var result = await _service.GetSessionsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("S1", result[0].Title);
    }

    [Fact]
    public async Task CreateSessionAsync_SendsPostRequest()
    {
        // Arrange
        var request = new CreateSessionRequest { Title = "NewSession" };
        SetupRequest("POST", "api/sessions/create", null);

        // Act
        await _service.CreateSessionAsync(request);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/sessions/create")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsSession()
    {
        // Arrange
        var id = Guid.NewGuid();
        var session = new SessionDto { Id = id, Title = "Session1" };
        SetupRequest("GET", $"api/sessions/{id}", session);

        // Act
        var result = await _service.GetSessionAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Session1", result!.Title);
    }

    [Fact]
    public async Task DeleteSessionAsync_SendsDeleteRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        SetupRequest("DELETE", $"api/sessions/{id}", null);

        // Act
        await _service.DeleteSessionAsync(id);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("DELETE") && req.RequestUri!.ToString().Contains($"api/sessions/{id}")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SaveSessionsAsync_SendsPostRequest()
    {
        // Arrange
        SetupRequest("POST", "api/sessions/save", null);

        // Act
        await _service.SaveSessionsAsync();

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/sessions/save")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ClearSessionsAsync_SendsDeleteRequest()
    {
        // Arrange
        SetupRequest("DELETE", "api/sessions/clear", null);

        // Act
        await _service.ClearSessionsAsync();

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("DELETE") && req.RequestUri!.ToString().Contains("api/sessions/clear")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExportSessionAsync_SendsGetRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        SetupRequest("GET", $"api/sessions/{id}/export", null);

        // Act
        await _service.ExportSessionAsync(id);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("GET") && req.RequestUri!.ToString().Contains($"api/sessions/{id}/export")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ImportSessionAsync_SendsPostRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var content = new byte[] { 1, 2, 3 };
        SetupRequest("POST", $"api/sessions/{id}/import", null);

        // Act
        await _service.ImportSessionAsync(id, content);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains($"api/sessions/{id}/import")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ExportAllSessionsAsync_SendsGetRequest()
    {
        // Arrange
        SetupRequest("GET", "api/sessions/export-all", null);

        // Act
        await _service.ExportAllSessionsAsync();

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("GET") && req.RequestUri!.ToString().Contains("api/sessions/export-all")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task ImportAllSessionsAsync_SendsPostRequest()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3 };
        SetupRequest("POST", "api/sessions/import-all", null);

        // Act
        await _service.ImportAllSessionsAsync(content);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("POST") && req.RequestUri!.ToString().Contains("api/sessions/import-all")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateSessionWorkingFolderAsync_SendsPutRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateSessionWorkingFolderRequest { WorkingFolder = "/tmp" };
        SetupRequest("PUT", $"api/sessions/{id}/working-folder", null);

        // Act
        await _service.UpdateSessionWorkingFolderAsync(id, request);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("PUT") && req.RequestUri!.ToString().Contains($"api/sessions/{id}/working-folder")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetSystemPromptsAsync_ReturnsPrompts()
    {
        // Arrange
        var prompts = new List<SystemPrompt> { new SystemPrompt { Id = 1, Text = "Prompt1" } };
        SetupRequest("GET", "api/SystemPrompt/list", prompts);

        // Act
        var result = await _service.GetSystemPromptsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Prompt1", result[0].Text);
    }

    [Fact]
    public async Task CreateOrUpdateSystemPromptAsync_ReturnsPrompt()
    {
        // Arrange
        var request = new SystemPromptRequest { Text = "New Prompt" };
        var response = new SystemPrompt { Id = 1, Text = "New Prompt" };
        SetupRequest("PUT", "api/SystemPrompt", response);

        // Act
        var result = await _service.CreateOrUpdateSystemPromptAsync(request);

        // Assert
        Assert.Equal("New Prompt", result.Text);
    }

    [Fact]
    public async Task GetSystemPromptAsync_ReturnsPrompt()
    {
        // Arrange
        var prompt = new SystemPrompt { Id = 1, Text = "Prompt1" };
        SetupRequest("GET", "api/SystemPrompt/1", prompt);

        // Act
        var result = await _service.GetSystemPromptAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Prompt1", result!.Text);
    }

    [Fact]
    public async Task UpdateSystemPromptAsync_SendsPutRequest()
    {
        // Arrange
        var request = new SystemPromptRequest { Text = "Updated Prompt" };
        SetupRequest("PUT", "api/SystemPrompt/1", null);

        // Act
        await _service.UpdateSystemPromptAsync(1, request);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("PUT") && req.RequestUri!.ToString().Contains("api/SystemPrompt/1")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task DeleteSystemPromptAsync_SendsDeleteRequest()
    {
        // Arrange
        SetupRequest("DELETE", "api/SystemPrompt/1", null);

        // Act
        await _service.DeleteSystemPromptAsync(1);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("DELETE") && req.RequestUri!.ToString().Contains("api/SystemPrompt/1")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateUserPromptTextAsync_SendsPutRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        var request = new UpdateUserPromptTextRequest { Text = "New Text" };
        SetupRequest("PUT", $"api/user-prompts/{sessionId}/{promptId}/text", null);

        // Act
        await _service.UpdateUserPromptTextAsync(sessionId, promptId, request);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("PUT") && req.RequestUri!.ToString().Contains($"api/user-prompts/{sessionId}/{promptId}/text")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SetUserPromptReadyAsync_SendsPatchRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        SetupRequest("PATCH", $"api/user-prompts/{sessionId}/{promptId}/status/Ready", null);

        // Act
        await _service.SetUserPromptReadyAsync(sessionId, promptId);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("PATCH") && req.RequestUri!.ToString().Contains($"api/user-prompts/{sessionId}/{promptId}/status/Ready")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task SetUserPromptUnreadyAsync_SendsPatchRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var promptId = Guid.NewGuid();
        SetupRequest("PATCH", $"api/user-prompts/{sessionId}/{promptId}/status/Draft", null);

        // Act
        await _service.SetUserPromptUnreadyAsync(sessionId, promptId);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == new HttpMethod("PATCH") && req.RequestUri!.ToString().Contains($"api/user-prompts/{sessionId}/{promptId}/status/Draft")),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
