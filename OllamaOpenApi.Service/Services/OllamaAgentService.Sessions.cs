using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        // Session Management
        public Task<List<Session>> ListSessionsAsync() => Task.FromResult(_executionList.Values.ToList());

        public Task<Session?> GetSessionAsync(Guid sessionId)
        {
            _executionList.TryGetValue(sessionId, out var session);
            return Task.FromResult(session);
        }

        public Task<Guid> CreateSessionAsync(string? model, int? systemPromptId, int thinkLevel, string? prompt, string? title, string? systemPromptText, string? workingFolder)
        {
            var sessionId = Guid.NewGuid();
            var session = new Session
            {
                Id = sessionId,
                Title = title ?? sessionId.ToString(),
                Model = model,
                ThinkLevel = thinkLevel,
                SystemPromptId = systemPromptId,
                WorkingFolder = workingFolder ?? string.Empty,
            };

            if (systemPromptId.HasValue)
            {
                var sp = _systemPrompts.FirstOrDefault(p => p.Id == systemPromptId.Value);
                if (sp != null)
                {
                    session.SystemPrompt = sp.Text;
                }
            }

            if (!string.IsNullOrWhiteSpace(systemPromptText))
            {
                session.SystemPrompt = systemPromptText;
            }

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                session.UserPrompts.Add(new UserPrompt { UserPromptText = prompt });
            }

            _executionList[session.Id] = session;
            return Task.FromResult(session.Id);
        }

        public Task UpdateSessionSystemPromptAsync(Guid sessionId, string prompt)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                session.SystemPrompt = prompt;
            }
            return Task.CompletedTask;
        }

        public Task UpdateSessionTitleAsync(Guid sessionId, string title)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                session.Title = title;
            }
            return Task.CompletedTask;
        }

        public Task UpdateSessionWorkingFolderAsync(Guid sessionId, string workingFolder)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                session.WorkingFolder = workingFolder;
            }
            return Task.CompletedTask;
        }

        public Task UpdateSessionPriorityAsync(Guid sessionId, int priority)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                session.Priority = priority;
            }
            return Task.CompletedTask;
        }

        public Task<byte[]> ExportSessionAsync(Guid sessionId)
        {
            if (!_executionList.TryGetValue(sessionId, out var session))
                throw new KeyNotFoundException($"Session '{sessionId}' was not found.");

            var json = JsonSerializer.Serialize(session, JsonOptions);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }

        public Task ImportSessionAsync(byte[] data)
        {
            if (data is null || data.Length == 0)
                throw new ArgumentException("Session payload is empty.", nameof(data));

            var session = JsonSerializer.Deserialize<Session>(Encoding.UTF8.GetString(data), JsonOptions)
                          ?? throw new ArgumentException("Could not deserialize session payload.", nameof(data));

            if (session.Id == Guid.Empty)
                session.Id = Guid.NewGuid();

            if (string.IsNullOrEmpty(session.Title))
                session.Title = session.Id.ToString();

            _executionList[session.Id] = session;
            return Task.CompletedTask;
        }

        public Task<byte[]> ExportAllSessionsAsync()
        {
            var payload = new SessionExportEnvelope
            {
                Version = 1,
                ExportedAt = DateTime.UtcNow,
                Sessions = _executionList.Values.ToList()
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }

        public Task ImportAllSessionsAsync(byte[] data)
        {
            if (data is null || data.Length == 0)
                throw new ArgumentException("Session payload is empty.", nameof(data));

            var envelope = JsonSerializer.Deserialize<SessionExportEnvelope>(Encoding.UTF8.GetString(data), JsonOptions)
                           ?? throw new ArgumentException("Could not deserialize sessions payload.", nameof(data));

            if (envelope.Sessions is null)
                return Task.CompletedTask;

            foreach (var session in envelope.Sessions)
            {
                if (session.Id == Guid.Empty)
                    session.Id = Guid.NewGuid();

                if (string.IsNullOrEmpty(session.Title))
                    session.Title = session.Id.ToString();

                _executionList[session.Id] = session;
            }

            return Task.CompletedTask;
        }

        public Task DeleteSessionAsync(Guid sessionId)
        {
            _executionList.TryRemove(sessionId, out _);
            return Task.CompletedTask;
        }

        public Task ArchiveSessionAsync(Guid sessionId, bool archive)
        {
            if (_executionList.TryGetValue(sessionId, out var session))
            {
                session.Archived = archive;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAllSessionsAsync()
        {
            _executionList.Clear();
            return Task.CompletedTask;
        }

        public async Task SaveSessionsToDiskAsync()
        {
            try
            {
                var data = await ExportAllSessionsAsync();
                await File.WriteAllBytesAsync(SessionFilePath, data);
                _logger.LogInformation("Successfully saved sessions to {Path}", SessionFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save sessions to {Path}", SessionFilePath);
            }
        }

        private async Task LoadSessionsFromDiskAsync()
        {
            _logger.LogInformation("OllamaAgentService.LoadSessionsFromDiskAsync begin");
            var path = SessionFilePath;
            if (!File.Exists(path))
            {
                _logger.LogInformation("Session persistence file not found at {Path}. Skipping load.", path);
                return;
            }

            try
            {
                var data = await File.ReadAllBytesAsync(path);
                await ImportAllSessionsAsync(data);
                _logger.LogInformation("Successfully loaded sessions from {Path}", path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load sessions from {Path}", path);
            }

            _logger.LogInformation("OllamaAgentService.LoadSessionsFromDiskAsync end");
        }
    }
}
