using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;

namespace OllamaOpenApi.Service.Tests
{
    public class MockOllamaAgentService : IOllamaAgentService
    {
        private readonly List<SystemPrompt> _systemPrompts = new();
        private readonly List<Session> _sessions = new();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public Task<List<ModelInfo>> GetModelsAsync() => Task.FromResult(new List<ModelInfo>());
        public Task<ModelInfo?> GetDefaultModelAsync() => Task.FromResult<ModelInfo?>(null);
        public Task SetDefaultModelAsync(string modelName) => Task.CompletedTask;
        public Task LoadModelAsync(string model) => Task.CompletedTask;
        public Task UnloadModelAsync(string model) => Task.CompletedTask;
        public Task ReloadModelAsync(string model) => Task.CompletedTask;

        public Task<List<Session>> ListSessionsAsync() => Task.FromResult(_sessions);
        public Task<Guid> CreateSessionAsync(string? model, int? systemPromptId, int thinkLevel, string? prompt, string? title, string? systemPromptText, string? workingFolder)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Model = model,
                SystemPromptId = systemPromptId,
                Title = title ?? "New Session",
                WorkingFolder = workingFolder ?? string.Empty
            };
            _sessions.Add(session);
            return Task.FromResult(session.Id);
        }
        public Task UpdateSessionSystemPromptAsync(Guid sessionId, string prompt) => Task.CompletedTask;
        public Task UpdateSessionTitleAsync(Guid sessionId, string title)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null) session.Title = title;
            return Task.CompletedTask;
        }
        public Task UpdateSessionWorkingFolderAsync(Guid sessionId, string workingFolder)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null) session.WorkingFolder = workingFolder;
            return Task.CompletedTask;
        }

        public Task UpdateSessionPriorityAsync(Guid sessionId, int priority)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session != null) session.Priority = priority;
            return Task.CompletedTask;
        }

        public Task<byte[]> ExportSessionAsync(Guid sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) throw new KeyNotFoundException();
            var json = JsonSerializer.Serialize(session);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }

        public Task ImportSessionAsync(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var session = JsonSerializer.Deserialize<Session>(json, _jsonOptions);
            if (session != null)
            {
                _sessions.RemoveAll(s => s.Id == session.Id);
                _sessions.Add(session);
            }
            return Task.CompletedTask;
        }

        public Task<byte[]> ExportAllSessionsAsync()
        {
            var envelope = new { Version = "1.0", Sessions = _sessions };
            var json = JsonSerializer.Serialize(envelope);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }

        public Task ImportAllSessionsAsync(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var envelope = JsonSerializer.Deserialize<SessionEnvelope>(json, _jsonOptions);
            if (envelope?.Sessions != null)
            {
                _sessions.Clear();
                _sessions.AddRange(envelope.Sessions);
            }
            return Task.CompletedTask;
        }

        public Task DeleteSessionAsync(Guid sessionId)
        {
            _sessions.RemoveAll(s => s.Id == sessionId);
            return Task.CompletedTask;
        }
        public Task ArchiveSessionAsync(Guid sessionId, bool archive)
        {
            _sessions.SingleOrDefault(s => s.Id == sessionId)?.Archived = archive;
            return Task.CompletedTask;
        }
        public Task DeleteAllSessionsAsync()
        {
            _sessions.Clear();
            return Task.CompletedTask;
        }
        public Task SaveSessionsToDiskAsync() => Task.CompletedTask;

        public Task<Guid> SubmitQueryAsync(string userPrompt, Guid? sessionId)
        {
            Guid actualSessionId = sessionId ?? Guid.NewGuid();
            var session = _sessions.FirstOrDefault(s => s.Id == actualSessionId);
            if (session == null)
            {
                session = new Session { Id = actualSessionId, Title = "Query Session" };
                _sessions.Add(session);
            }

            session.UserPrompts.Add(new UserPrompt { UserPromptText = userPrompt });
            return Task.FromResult(actualSessionId);
        }

        public Task AddToExecutionListAsync(List<Guid> sessionIds) => Task.CompletedTask;
        public Task PauseExecutionListAsync() => Task.CompletedTask;
        public Task ResumeExecutionListAsync() => Task.CompletedTask;
        public Task<Session?> GetActiveProcessingSessionAsync() => Task.FromResult<Session?>(null);
        public Task InterruptActiveSessionAsync() => Task.CompletedTask;
        public ExecutionList GetExecutionList() => new ExecutionList();

        public Task<bool> AddUserPromptAsync(Guid sessionId, string text) => Task.FromResult(true);
        public Task<bool> UpdateUserPromptTextAsync(Guid sessionId, Guid promptId, string text) => Task.FromResult(true);
        public Task<bool> SetUserPromptStatusAsync(Guid sessionId, Guid promptId, UserPromptStatus status) => Task.FromResult(true);
        public Task<bool> DeleteUserPromptAsync(Guid sessionId, Guid promptId) => Task.FromResult(true);

        public Task<List<SystemPrompt>> GetSystemPromptsAsync() => Task.FromResult(_systemPrompts);
        public Task AddSystemPromptAsync(SystemPrompt prompt)
        {
            prompt.Id = _systemPrompts.Any() ? _systemPrompts.Max(p => p.Id) + 1 : 1;
            _systemPrompts.Add(prompt);
            return Task.CompletedTask;
        }
        public Task UpdateSystemPromptAsync(SystemPrompt prompt)
        {
            var existing = _systemPrompts.FirstOrDefault(p => p.Id == prompt.Id);
            if (existing != null)
            {
                existing.Text = prompt.Text;
            }
            return Task.CompletedTask;
        }
        public Task DeleteSystemPromptAsync(int id)
        {
            _systemPrompts.RemoveAll(p => p.Id == id);
            return Task.CompletedTask;
        }

        public Task<string> GetOllamaStatusAsync() => Task.FromResult("{\"models\":[]}");

        private class SessionEnvelope
        {
            public object Version { get; set; } = string.Empty;
            public List<Session> Sessions { get; set; } = new();
        }
    }
}
