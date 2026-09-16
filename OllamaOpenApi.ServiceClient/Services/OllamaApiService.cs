using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using OllamaOpenApi.Dto;
using System.Linq;

namespace OllamaOpenApi.ServiceClient.Services;

public class OllamaApiService : IOllamaApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaApiService> _logger;

    public OllamaApiService(HttpClient httpClient, ILogger<OllamaApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private async Task<T?> GetFromJsonAsyncSafe<T>(string url)
    {
        _logger.LogDebug("Calling GET {Url} for type {Type}", url, typeof(T).Name);
        var response = await _httpClient.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("GET {Url} returned NotFound", url);
            return default;
        }
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>();
        _logger.LogDebug("GET {Url} returned successfully", url);
        return result;
    }

    // Models
    public async Task<List<ModelDto>> GetModelsAsync()
    {
        _logger.LogDebug("Calling GetModelsAsync");
        var result = await _httpClient.GetFromJsonAsync<List<ModelDto>>("api/models/list");
        _logger.LogDebug("GetModelsAsync returned {Count} models", result?.Count ?? 0);
        return result ?? new();
    }

    public async Task<ModelDto?> GetDefaultModelAsync()
    {
        _logger.LogDebug("Calling GetDefaultModelAsync");
        var result = await GetFromJsonAsyncSafe<ModelDto>("api/models/default");
        _logger.LogDebug("GetDefaultModelAsync returned {Result}", result?.Name ?? "null");
        return result;
    }

    public async Task SetDefaultModelAsync(SetModelRequest request)
    {
        _logger.LogDebug("Calling SetDefaultModelAsync with request: {Request}", request);
        await _httpClient.PutAsJsonAsync("api/models/default", request);
        _logger.LogDebug("SetDefaultModelAsync completed");
    }

    // Status
    public async Task<List<OllamaStatusDto>> GetOllamaStatusAsync()
    {
        _logger.LogDebug("Calling GetOllamaStatusAsync");
        var response = await _httpClient.GetFromJsonAsync<OllamaStatusResponseDto>("api/ollama/ps");
        var models = response?.Models?.ToList() ?? new List<OllamaStatusDto>();
        _logger.LogDebug("GetOllamaStatusAsync returned {Count} models", models.Count);
        return models;
    }

    // Processing
    public async Task PauseExecutionListAsync()
    {
        _logger.LogDebug("Calling PauseExecutionListAsync");
        await _httpClient.PostAsync("api/Processing/pause", null);
        _logger.LogDebug("PauseExecutionListAsync completed");
    }

    public async Task ResumeExecutionListAsync()
    {
        _logger.LogDebug("Calling ResumeExecutionListAsync");
        await _httpClient.PostAsync("api/Processing/resume", null);
        _logger.LogDebug("ResumeExecutionListAsync completed");
    }

    public async Task InterruptExecutionListAsync()
    {
        _logger.LogDebug("Calling InterruptExecutionListAsync");
        await _httpClient.PostAsync("api/Processing/interrupt", null);
        _logger.LogDebug("InterruptExecutionListAsync completed");
    }

    public async Task<SessionDto?> GetActiveExecutionSessionAsync()
    {
        _logger.LogDebug("Calling GetActiveExecutionSessionAsync");
        var result = await GetFromJsonAsyncSafe<SessionDto>("api/Processing/activeSession");
        _logger.LogDebug("GetActiveExecutionSessionAsync returned {Result}", result?.Id.ToString() ?? "null");
        return result;
    }

    public async Task<string> GetExecutionListStatusAsync()
    {
        _logger.LogDebug("Calling GetExecutionListStatusAsync");
        var result = await _httpClient.GetFromJsonAsync<ExecutionListStatusResponseDto>("api/Processing/status");
        _logger.LogDebug("GetExecutionListStatusAsync returned: {Result}", result?.Status);
        return result?.Status.ToString() ?? "Unknown";
    }

    // Ollama Management
    public async Task LoadModelAsync(string model)
    {
        _logger.LogDebug("Calling LoadModelAsync for model: {Model}", model);
        await _httpClient.PostAsync($"api/ollama/load/{model}", null);
        _logger.LogDebug("LoadModelAsync for {Model} completed", model);
    }

    public async Task UnloadModelAsync(string model)
    {
        _logger.LogDebug("Calling UnloadModelAsync for model: {Model}", model);
        await _httpClient.PostAsync($"api/ollama/unload/{model}", null);
        _logger.LogDebug("UnloadModelAsync for {Model} completed", model);
    }

    public async Task ReloadModelAsync(string model)
    {
        _logger.LogDebug("Calling ReloadModelAsync for model: {Model}", model);
        await _httpClient.PostAsync($"api/ollama/reload/{model}", null);
        _logger.LogDebug("ReloadModelAsync for {Model} completed", model);
    }

    // Sessions
    public async Task<List<SessionDto>> GetSessionsAsync()
    {
        _logger.LogDebug("Calling GetSessionsAsync");
        var result = await _httpClient.GetFromJsonAsync<List<SessionDto>>("api/sessions/list");
        _logger.LogDebug("GetSessionsAsync returned {Count} sessions", result?.Count ?? 0);
        return result ?? new();
    }

    public async Task CreateSessionAsync(CreateSessionRequest request)
    {
        _logger.LogDebug("Calling CreateSessionAsync with request: {Request}", request);
        await _httpClient.PostAsJsonAsync("api/sessions/create", request);
        _logger.LogDebug("CreateSessionAsync completed");
    }

    public async Task<SessionDto?> GetSessionAsync(Guid id)
    {
        _logger.LogDebug("Calling GetSessionAsync for id: {Id}", id);
        var result = await GetFromJsonAsyncSafe<SessionDto>($"api/sessions/{id}");
        _logger.LogDebug("GetSessionAsync for {Id} returned {Result}", id, result?.Title ?? "null");
        return result;
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        _logger.LogDebug("Calling DeleteSessionAsync for id: {Id}", id);
        await _httpClient.DeleteAsync($"api/sessions/{id}");
        _logger.LogDebug("DeleteSessionAsync for {Id} completed", id);
    }

    public async Task ArchiveSessionAsync(Guid id)
    {
        _logger.LogDebug("Calling ArchiveSessionAsync for id: {Id}", id);
        await _httpClient.PutAsync($"api/sessions/{id}/archive", null);
        _logger.LogDebug("ArchiveSessionAsync for {Id} completed", id);
    }

    public async Task UnarchiveSessionAsync(Guid id)
    {
        _logger.LogDebug("Calling UnarchiveSessionAsync for id: {Id}", id);
        await _httpClient.PutAsync($"api/sessions/{id}/unarchive", null);
        _logger.LogDebug("UnarchiveSessionAsync for {Id} completed", id);
    }

    public async Task SaveSessionsAsync()
    {
        _logger.LogDebug("Calling SaveSessionsAsync");
        await _httpClient.PostAsync("api/sessions/save", null);
        _logger.LogDebug("SaveSessionsAsync completed");
    }

    public async Task ClearSessionsAsync()
    {
        _logger.LogDebug("Calling ClearSessionsAsync");
        await _httpClient.DeleteAsync("api/sessions/clear");
        _logger.LogDebug("ClearSessionsAsync completed");
    }

    public async Task ExportSessionAsync(Guid id)
    {
        _logger.LogDebug("Calling ExportSessionAsync for id: {Id}", id);
        await _httpClient.GetAsync($"api/sessions/{id}/export");
        _logger.LogDebug("ExportSessionAsync for {Id} completed", id);
    }

    public async Task ImportSessionAsync(Guid id, byte[] fileContent)
    {
        _logger.LogDebug("Calling ImportSessionAsync for id: {Id}, content length: {Length}", id, fileContent.Length);
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "session.json");
        await _httpClient.PostAsync($"api/sessions/{id}/import", content);
        _logger.LogDebug("ImportSessionAsync for {Id} completed", id);
    }

    public async Task ExportAllSessionsAsync()
    {
        _logger.LogDebug("Calling ExportAllSessionsAsync");
        await _httpClient.GetAsync("api/sessions/export-all");
        _logger.LogDebug("ExportAllSessionsAsync completed");
    }

    public async Task ImportAllSessionsAsync(byte[] fileContent)
    {
        _logger.LogDebug("Calling ImportAllSessionsAsync, content length: {Length}", fileContent.Length);
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "sessions.json");
        await _httpClient.PostAsync("api/sessions/import-all", content);
        _logger.LogDebug("ImportAllSessionsAsync completed");
    }

    public async Task UpdateSessionWorkingFolderAsync(Guid id, UpdateSessionWorkingFolderRequest request)
    {
        _logger.LogDebug("Calling UpdateSessionWorkingFolderAsync for id: {Id}, request: {Request}", id, request);
        await _httpClient.PutAsJsonAsync($"api/sessions/{id}/working-folder", request);
        _logger.LogDebug("UpdateSessionWorkingFolderAsync for {Id} completed", id);
    }

    public async Task UpdateSessionPriorityAsync(Guid id, UpdateSessionPriorityRequest request)
    {
        _logger.LogDebug("Calling UpdateSessionPriorityAsync for id: {Id}, request: {Request}", id, request);
        await _httpClient.PutAsJsonAsync($"api/sessions/{id}/priority", request);
        _logger.LogDebug("UpdateSessionPriorityAsync for {Id} completed", id);
    }

    // SystemPrompt
    public async Task<List<SystemPrompt>> GetSystemPromptsAsync()
    {
        _logger.LogDebug("Calling GetSystemPromptsAsync");
        var result = await _httpClient.GetFromJsonAsync<List<SystemPrompt>>("api/SystemPrompt/list");
        _logger.LogDebug("GetSystemPromptsAsync returned {Count} prompts", result?.Count ?? 0);
        return result ?? new();
    }

    public async Task<SystemPrompt> CreateOrUpdateSystemPromptAsync(SystemPromptRequest request)
    {
        _logger.LogDebug("Calling CreateOrUpdateSystemPromptAsync with request: {Request}", request);
        var response = await _httpClient.PutAsJsonAsync("api/SystemPrompt", request);
        var result = await response.Content.ReadFromJsonAsync<SystemPrompt>() ?? new();
        _logger.LogDebug("CreateOrUpdateSystemPromptAsync returned prompt with id: {Id}", result.Id);
        return result;
    }

    public async Task<SystemPrompt?> GetSystemPromptAsync(int id)
    {
        _logger.LogDebug("Calling GetSystemPromptAsync for id: {Id}", id);
        var result = await GetFromJsonAsyncSafe<SystemPrompt>($"api/SystemPrompt/{id}");
        _logger.LogDebug("GetSystemPromptAsync for {Id} returned {Result}", id, result?.Text ?? "null");
        return result;
    }

    public async Task UpdateSystemPromptAsync(int id, SystemPromptRequest request)
    {
        _logger.LogDebug("Calling UpdateSystemPromptAsync for id: {Id}, request: {Request}", id, request);
        await _httpClient.PutAsJsonAsync($"api/SystemPrompt/{id}", request);
        _logger.LogDebug("UpdateSystemPromptAsync for {Id} completed", id);
    }

    public async Task DeleteSystemPromptAsync(int id)
    {
        _logger.LogDebug("Calling DeleteSystemPromptAsync for id: {Id}", id);
        await _httpClient.DeleteAsync($"api/SystemPrompt/{id}");
        _logger.LogDebug("DeleteSystemPromptAsync for {Id} completed", id);
    }

    // UserPrompt
    public async Task<List<FolderDto>> GetFoldersAsync(string? path)
    {
        _logger.LogDebug("Calling GetFoldersAsync for path: {Path}", path);
        var url = $"api/folders/list?path={System.Net.WebUtility.UrlEncode(path ?? "")}";
        var result = await _httpClient.GetFromJsonAsync<List<FolderDto>>(url);
        _logger.LogDebug("GetFoldersAsync returned {Count} items", result?.Count ?? 0);
        return result ?? new();
    }

    public async Task AddUserPromptAsync(Guid sessionId, UpdateUserPromptTextRequest request)
    {
        _logger.LogDebug("Calling AddUserPromptAsync for session: {SessionId}, request: {Request}", sessionId, request);
        await _httpClient.PostAsJsonAsync($"api/user-prompts/{sessionId}/text", request);
        _logger.LogDebug("AddUserPromptAsync for session {SessionId} completed", sessionId);
    }

    public async Task UpdateUserPromptTextAsync(Guid sessionId, Guid promptId, UpdateUserPromptTextRequest request)
    {
        _logger.LogDebug("Calling UpdateUserPromptTextAsync for session: {SessionId}, prompt: {PromptId}, request: {Request}", sessionId, promptId, request);
        await _httpClient.PutAsJsonAsync($"api/user-prompts/{sessionId}/{promptId}/text", request);
        _logger.LogDebug("UpdateUserPromptTextAsync for prompt {PromptId} completed", promptId);
    }

    public async Task SetUserPromptReadyAsync(Guid sessionId, Guid promptId)
    {
        _logger.LogDebug("Calling SetUserPromptReadyAsync for session: {SessionId}, prompt: {PromptId}", sessionId, promptId);
        await _httpClient.PatchAsync($"api/user-prompts/{sessionId}/{promptId}/status/Ready", null);
        _logger.LogDebug("SetUserPromptReadyAsync for prompt {PromptId} completed", promptId);
    }

    public async Task SetUserPromptUnreadyAsync(Guid sessionId, Guid promptId)
    {
        _logger.LogDebug("Calling SetUserPromptUnreadyAsync for session: {SessionId}, prompt: {PromptId}", sessionId, promptId);
        await _httpClient.PatchAsync($"api/user-prompts/{sessionId}/{promptId}/status/Draft", null);
        _logger.LogDebug("SetUserPromptUnreadyAsync for prompt {PromptId} completed", promptId);
    }
}
