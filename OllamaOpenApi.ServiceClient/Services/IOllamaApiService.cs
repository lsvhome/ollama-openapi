using OllamaOpenApi.Dto;

namespace OllamaOpenApi.ServiceClient.Services;

public interface IOllamaApiService
{
    // Models
    Task<List<ModelDto>> GetModelsAsync();
    Task<ModelDto?> GetDefaultModelAsync();
    Task SetDefaultModelAsync(SetModelRequest request);

    // Status
    Task<List<OllamaStatusDto>> GetOllamaStatusAsync();

    // Processing
    Task PauseExecutionListAsync();
    Task ResumeExecutionListAsync();
    Task InterruptExecutionListAsync();
    Task<SessionDto?> GetActiveExecutionSessionAsync();
    Task<string> GetExecutionListStatusAsync();

    // Ollama Management
    Task LoadModelAsync(string model);
    Task UnloadModelAsync(string model);
    Task ReloadModelAsync(string model);

    // Sessions
    Task<List<SessionDto>> GetSessionsAsync();
    Task CreateSessionAsync(CreateSessionRequest request);
    Task<SessionDto?> GetSessionAsync(Guid id);
    Task DeleteSessionAsync(Guid id);
    Task ArchiveSessionAsync(Guid id);
    Task UnarchiveSessionAsync(Guid id);
    Task SaveSessionsAsync();
    Task ClearSessionsAsync();
    Task ExportSessionAsync(Guid id);
    Task ImportSessionAsync(Guid id, byte[] fileContent);
    Task ExportAllSessionsAsync();
    Task ImportAllSessionsAsync(byte[] fileContent);
    Task UpdateSessionWorkingFolderAsync(Guid id, UpdateSessionWorkingFolderRequest request);
    Task UpdateSessionPriorityAsync(Guid id, UpdateSessionPriorityRequest request);

    // SystemPrompt
    Task<List<SystemPrompt>> GetSystemPromptsAsync();
    Task<SystemPrompt> CreateOrUpdateSystemPromptAsync(SystemPromptRequest request);
    Task<SystemPrompt?> GetSystemPromptAsync(int id);
    Task UpdateSystemPromptAsync(int id, SystemPromptRequest request);
    Task DeleteSystemPromptAsync(int id);

    // UserPrompt
    Task<List<FolderDto>> GetFoldersAsync(string? path);
    Task AddUserPromptAsync(Guid sessionId, UpdateUserPromptTextRequest request);
    Task UpdateUserPromptTextAsync(Guid sessionId, Guid promptId, UpdateUserPromptTextRequest request);
    Task SetUserPromptReadyAsync(Guid sessionId, Guid promptId);
    Task SetUserPromptUnreadyAsync(Guid sessionId, Guid promptId);
}
