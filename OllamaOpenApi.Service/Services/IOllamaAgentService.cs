using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public interface IOllamaAgentService
    {
        // Model Management
        Task<List<ModelInfo>> GetModelsAsync();
        Task<ModelInfo?> GetDefaultModelAsync();
        Task SetDefaultModelAsync(string modelName);
        Task LoadModelAsync(string model);
        Task UnloadModelAsync(string model);
        Task ReloadModelAsync(string model);

        // Session Management
        Task<List<Session>> ListSessionsAsync();
        Task<Guid> CreateSessionAsync(string? model, int? systemPromptId, int thinkLevel, string? prompt, string? title, string? systemPromptText, string? workingFolder);
        Task UpdateSessionSystemPromptAsync(Guid sessionId, string prompt);
        Task UpdateSessionTitleAsync(Guid sessionId, string title);
        Task UpdateSessionWorkingFolderAsync(Guid sessionId, string workingFolder);
        Task UpdateSessionPriorityAsync(Guid sessionId, int priority);
        Task<byte[]> ExportSessionAsync(Guid sessionId);
        Task ImportSessionAsync(byte[] data);
        Task<byte[]> ExportAllSessionsAsync();
        Task ImportAllSessionsAsync(byte[] data);
        Task DeleteSessionAsync(Guid sessionId);
        Task ArchiveSessionAsync(Guid sessionId, bool archive);
        Task DeleteAllSessionsAsync();
        Task SaveSessionsToDiskAsync();

        // Operational Management
        Task<Guid> SubmitQueryAsync(string userPrompt, Guid? sessionId);
        Task AddToExecutionListAsync(List<Guid> sessionIds);
        Task PauseExecutionListAsync();
        Task ResumeExecutionListAsync();
        Task<Session?> GetActiveProcessingSessionAsync();
        Task InterruptActiveSessionAsync();
        ExecutionList GetExecutionList();

        // User Prompt Management
        Task<bool> AddUserPromptAsync(Guid sessionId, string text);
        Task<bool> UpdateUserPromptTextAsync(Guid sessionId, Guid promptId, string text);
        Task<bool> SetUserPromptStatusAsync(Guid sessionId, Guid promptId, UserPromptStatus status);
        Task<bool> DeleteUserPromptAsync(Guid sessionId, Guid promptId);

        // System Prompt Management
        Task<List<SystemPrompt>> GetSystemPromptsAsync();
        Task AddSystemPromptAsync(SystemPrompt prompt);
        Task UpdateSystemPromptAsync(SystemPrompt prompt);
        Task DeleteSystemPromptAsync(int id);

        // Status Management
        Task<string> GetOllamaStatusAsync();
    }
}
