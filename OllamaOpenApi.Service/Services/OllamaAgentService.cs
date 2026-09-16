using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OllamaOpenApi.Dto;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService : BackgroundService, IOllamaAgentService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        private readonly IConfiguration _configuration;
        private readonly ILogger<OllamaAgentService> _logger;
        private OllamaSharp.IOllamaApiClient? _ollamaClient;

        private readonly List<ModelInfo> _models = new();
        private ExecutionList _executionList = new()!;
        private CancellationTokenSource _activeSessionCts = new();

        public OllamaAgentService(IConfiguration configuration, ILogger<OllamaAgentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string SessionFilePath => _configuration["SessionFilePath"] ?? "sessions.json";

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OllamaAgentService.StartAsync begin");
            InitializeDefaultPrompts();
            await LoadSessionsFromDiskAsync();
            await base.StartAsync(cancellationToken);
            _logger.LogInformation("OllamaAgentService.StartAsync end");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OllamaAgentService.StopAsync begin");
            await SaveSessionsToDiskAsync();
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("OllamaAgentService.StopAsync end");
        }

        // Operational Management
        public Task<Guid> SubmitQueryAsync(string userPrompt, Guid? sessionId)
        {
            var sId = sessionId ?? Guid.NewGuid();
            var session = _executionList.GetOrAdd(sId, _ => new Session { Id = sId });

            var query = new UserPrompt { UserPromptText = userPrompt, Status = UserPromptStatus.Ready };
            session.UserPrompts.Add(query);

            return Task.FromResult(sId);
        }

        public Task AddToExecutionListAsync(List<Guid> sessionIds)
        {
            foreach (var id in sessionIds)
            {
                if (_executionList.TryGetValue(id, out var session))
                {
                    // No longer need to set session.Status to Ready
                }
            }
            return Task.CompletedTask;
        }

        public Task PauseExecutionListAsync()
        {
            _executionList.Status = ExecutionListStatus.Paused;
            return Task.CompletedTask;
        }
        public Task ResumeExecutionListAsync()
        {
            _executionList.Status = ExecutionListStatus.InProgress;
            return Task.CompletedTask;
        }

        public async Task<Session?> GetActiveProcessingSessionAsync()
        {
            if (_executionList.ActiveSessionId.HasValue && _executionList.TryGetValue(_executionList.ActiveSessionId.Value, out var session))
                return session;
            return null;
        }
        public Task InterruptActiveSessionAsync()
        {
            _activeSessionCts.Cancel();
            _activeSessionCts = new CancellationTokenSource();
            return Task.CompletedTask;
        }

        public ExecutionList GetExecutionList() => _executionList;
    }

    internal sealed class SessionExportEnvelope
    {
        public int Version { get; set; } = 1;
        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
        public List<Session> Sessions { get; set; } = new();
    }
}
