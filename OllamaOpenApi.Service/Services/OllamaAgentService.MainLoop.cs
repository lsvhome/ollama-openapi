using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Tools;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OllamaAgentService.ExecuteAsync begin");
            // 1. Initialization
            var host = _configuration["ollama_host"];
            if (string.IsNullOrEmpty(host))
            {
                _logger.LogError("Configuration key 'ollama_host' is missing.");
                return;
            }


            _ollamaClient = new OllamaSharp.OllamaApiClient(
                new HttpClient() { BaseAddress = new Uri(host), Timeout = Timeout.InfiniteTimeSpan });

            try
            {
                await LoadDefaultModelAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama instance at {Host}", host);
                return;
            }

            // AutoStartProcessing
            if (string.Equals(_configuration["AutoStartProcessing"], "true", StringComparison.InvariantCultureIgnoreCase))
            {
                _executionList.Status = ExecutionListStatus.InProgress;
            }

            // 2. Main Loop - until application shutdown
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_executionList.Status == ExecutionListStatus.InProgress)
                {
                    var sessionToProcess = _executionList.GetSessionsToProcess().FirstOrDefault();
                    if (sessionToProcess != null)
                    {
                        _executionList.ActiveSessionId = sessionToProcess.Id;
                        _logger.LogInformation("Starting process for session {SessionId}", sessionToProcess.Id);

                        using var stoppingTokenCombined = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _activeSessionCts.Token);
                        {
                            try
                            {
                                await ProcessSessionAsync(sessionToProcess, stoppingTokenCombined.Token);
                            }
                            catch (OperationCanceledException) // + catch (TaskCanceledException)
                            {
                                _logger.LogWarning("Processing for session {SessionId} was cancelled", sessionToProcess.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing session {SessionId}", sessionToProcess.Id);
                            }
                            finally
                            {
                                _executionList.ActiveSessionId = null;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No sessions ready to process.");
                    }
                }
                else
                {
                    _logger.LogDebug("Execution list is paused.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Normal shutdown
                }
            }

            _logger.LogInformation("OllamaAgentService.ExecuteAsync end");
        }

        private async Task ProcessSessionAsync(Session session, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Processing session {SessionId} with {QueryCount} queries.", session.Id, session.UserPrompts.Count(q => q.Status == UserPromptStatus.Ready || q.Status == UserPromptStatus.InProgress));

            string systemPromptText = session.SystemPrompt;
            if (string.IsNullOrEmpty(systemPromptText) && session.SystemPromptId.HasValue)
            {
                var prompt = _systemPrompts.FirstOrDefault(p => p.Id == session.SystemPromptId.Value);
                if (prompt != null) systemPromptText = prompt.Text;
            }

            var activeModel = await GetDefaultModelAsync();
            if (activeModel == null)
            {
                _logger.LogError("No default model configured for session {SessionId}", session.Id);
                throw new Exception("No default model configured.");
            }
            _logger.LogInformation("Using model {ModelName} for session {SessionId}", activeModel.Name, session.Id);

            var thinkValue = MapThinkLevel(session.ThinkLevel);
            var chat = new Chat(_ollamaClient!, systemPromptText) { Think = thinkValue };

            var completedMessages = session.UserPrompts.Where(s => s.Status == UserPromptStatus.Completed).SelectMany(s => s.Messages).OrderBy(m => m.Created);
            foreach (var each in completedMessages)
            {
                if (!chat.Messages.Any(m => m.Id == each.Id))
                {
                    chat.Messages.Add(each);
                }
            }

            chat.OnThink += (sender, thoughts) =>
            {
                try
                {
                    _logger.LogDebug("Append thoughts for session {SessionId} : [{Text}]", session.Id, thoughts.Text);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnThink");
                }
            };

            chat.OnThought += (sender, thoughts) =>
            {
                try
                {
                    _logger.LogDebug("Full thought for session {SessionId} : [{Text}]", session.Id, thoughts.Text);
                    if (thoughts.State is UserPrompt up)
                    {
                        up.Thoughts.Add(new Message(ChatRole.Assistant, thoughts.Text) { Thinking = thoughts.Text });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnThink");
                }
            };

            chat.OnToolCall += (sender, call) =>
            {
                try
                {
                    if (call == null) return;
                    _logger.LogDebug("Append toolcall for session {SessionId} : [{Text}]", session.Id, call.ToolCall?.Function?.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnToolCall");
                }
            };

            chat.OnToolResult += (sender, result) =>
            {
                try
                {
                    if (result == null) return;
                    _logger.LogDebug("Append toolresult for session {SessionId} : [{Text}]", session.Id, result.ToolResult?.Result?.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnToolResult");
                }
            };

            chat.OnMessage += (sender, msgArgs) =>
            {
                try
                {
                    if (msgArgs == null || msgArgs.Message == null) return;
                    _logger.LogDebug("Append messages for session {SessionId} : [{Text}]", session.Id, msgArgs.Message.Content);
                    if (msgArgs.State is UserPrompt up)
                    {
                        up.Messages.Add(msgArgs.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnMessage");
                }
            };

            chat.OnBeforeSend += (sender, msgArgs) =>
            {
                try
                {
                    if (msgArgs == null || string.IsNullOrWhiteSpace(msgArgs.Message.Content)) return;
                    _logger.LogDebug("Going to send chat message for session {SessionId} : [{Text}]", session.Id, msgArgs.Message.Content);

                    if (_executionList.Status != ExecutionListStatus.InProgress)
                    {
                        _logger.LogInformation("Execution list paused. Stopping processing of session {SessionId}.", session.Id);

                        _logger.LogDebug("Add not sent message into the message list before stop for session {SessionId} : [{Text}]", session.Id, msgArgs.Message.Content);

                        if (msgArgs.State is UserPrompt up)
                        {
                            up.Messages.Add(msgArgs.Message);
                        }

                        throw new OperationCanceledException();
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OnMessage");
                }
            };

            var tools = await GetAvailableToolsAsync(session.Id);

            var queriesToProcess = session.UserPrompts
                .Where(q => q.Status == UserPromptStatus.Ready || q.Status == UserPromptStatus.InProgress)
                .OrderByDescending(q => q.Status)
                .ThenBy(q => q.CreatedTimestamp)
                .ToList();

            if (!string.IsNullOrWhiteSpace(session.WorkingFolder) && Directory.Exists(session.WorkingFolder))
            {
                Directory.SetCurrentDirectory(session.WorkingFolder);
            }

            foreach (var q in queriesToProcess)
            {
                if (_executionList.Status != ExecutionListStatus.InProgress)
                {
                    _logger.LogInformation("Execution list paused. Stopping processing of session {SessionId}.", session.Id);
                    break;
                }

                if (q.Messages.Any())
                {
                    chat.Messages.AddRange(q.Messages.OrderBy(m => m.Created));

                    foreach (var each in q.Messages)
                    {
                        if (!chat.Messages.Any(m => m.Id == each.Id))
                        {
                            chat.Messages.Add(each);
                        }
                    }
                }

                try
                {
                    _logger.LogInformation("Processing query {QueryId} for session {SessionId}: {UserPromptText}", q.Id, session.Id, q.UserPromptText);

                    ChatRole senderRole = ChatRole.User;
                    string senderText = q.UserPromptText;

                    if (q.Attachments.Any())
                    {
                        var attachmentsContent = new System.Text.StringBuilder();
                        attachmentsContent.AppendLine("Attached files:");
                        foreach (var attachment in q.Attachments)
                        {
                            try
                            {
                                if (File.Exists(attachment.FilePath))
                                {
                                    var content = await File.ReadAllTextAsync(attachment.FilePath);
                                    attachmentsContent.AppendLine($"--- File: {attachment.FileName} ---\n{content}\n");
                                }
                                else
                                {
                                    attachmentsContent.AppendLine($"--- File: {attachment.FileName} (File not found) ---\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                attachmentsContent.AppendLine($"--- File: {attachment.FileName} (Error reading file: {ex.Message}) ---\n");
                            }
                        }
                        senderText = $"{senderText}\n\n{attachmentsContent}";
                    }

                    if (q.Status == UserPromptStatus.InProgress)
                    {
                        // message already sent, let's find latest message in queue which we consider as not sent in order to continue dialog
                        var msg = q.Messages.Where(m => m.Role.HasValue && m.Role.Value != ChatRole.Assistant).LastOrDefault();
                        if (msg != null && !string.IsNullOrWhiteSpace(msg.Content))
                        {
                            senderRole = msg.Role!.Value;
                            senderText = msg.Content;
                        }
                        else
                        {
                            // message already sent, but messages queue is empty - wrong state, precessit it as Ready
                            q.Status = UserPromptStatus.InProgress;
                            q.SentTimestamp = DateTime.UtcNow;
                            _logger.LogError("Processing query {QueryId} for session {SessionId}: wrong state.", q.Id, session.Id);
                        }
                    }
                    else if (q.Status == UserPromptStatus.Ready)
                    {
                        q.Status = UserPromptStatus.InProgress;
                        q.SentTimestamp = DateTime.UtcNow;
                    }
                    else throw new NotImplementedException();

                    var responseEnumerable = chat.SendAsAsync(senderRole, senderText, tools: tools, imagesAsBase64: null, format: null, state: q, cancellationToken: stoppingToken);

                    var responseList = new List<string>();
                    await foreach (var part in responseEnumerable)
                    {
                        responseList.Add(part);
                        _logger.LogDebug("Append response {QueryId} for session {SessionId} : [{Text}]", q.Id, session.Id, part);
                    }

                    q.Response = string.Concat(responseList);
                    q.Status = UserPromptStatus.Completed;
                    q.CompletedTimestamp = DateTime.UtcNow;

                    _logger.LogInformation("Completed query {QueryId} for session {SessionId}", q.Id, session.Id);

                    if (_externalSource.CanProceess(session.ExternalSourceType))
                    {
                        await _externalSource.Process(session);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Current operation cancelled (by shutdown or interrupted by user) and future prompt processing should be paused
                    _executionList.Status = ExecutionListStatus.Paused;
                    _logger.LogError(ex, "Cancelled processing query {QueryId} for session {SessionId}", q.Id, session.Id);

                    // increase priority for session in order to reprocess it first by default.
                    session.Priority++;

                    // UserPrompt should say in InProgress state in order to be reprocessed next time.
                    if (q.Status != UserPromptStatus.InProgress)
                    {
                        _logger.LogError(ex, "Query {QueryId} for session {SessionId} in wrong state '{ActualState}' while expected to be in 'InProgress' state.", q.Id, session.Id, q.Status);
                    }

                    await this.SaveSessionsToDiskAsync();

                    break;
                }
                catch (Exception ex)
                {
                    // Future prompt processing should be paused until user input. In future, when workflow would
                    // be stabilized, can be left as it is - first session will be processed then.
                    _executionList.Status = ExecutionListStatus.Paused;

                    _logger.LogError(ex, "Error processing query {QueryId} for session {SessionId}", q.Id, session.Id);
                    q.Status = UserPromptStatus.Error;
                    break;
                }
                finally
                {
                    await this.SaveSessionsToDiskAsync();
                }
            }
        }

        // public static List<OllamaSharp.Models.Chat.Tool> Tools1 { get; } = [new FileWriteTool()];

        internal async Task<List<OllamaSharp.Models.Chat.Tool>> GetAvailableToolsAsync(Guid? sessionId = null)
        {
            List<OllamaSharp.Models.Chat.Tool> ret = new List<OllamaSharp.Models.Chat.Tool>();

            var allowLocalTools = _configuration.GetValue<bool>("allow-local-tools", true);

            if (allowLocalTools)
            {
                var localTools = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => typeof(OllamaSharp.Models.Chat.Tool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .Select(t => (OllamaSharp.Models.Chat.Tool?)Activator.CreateInstance(t))
                    .Where(t => t != null)
                    .Cast<OllamaSharp.Models.Chat.Tool>()
                    .Distinct()
                    .ToList();

                ret.AddRange(localTools);
            }

            try
            {
                var servers = await this.GetServersAsync(sessionId);
                foreach (var server in servers)
                {
                    if (server.Type != "http") continue;

                    var clientTransport = new HttpClientTransport(new()
                    {
                        Endpoint = new Uri(server.Url)
                    });

                    await using (var mcpClient = await McpClient.CreateAsync(clientTransport!))
                    {
                        var mcpTools = await mcpClient.ListToolsAsync();
                        foreach (var mcpTool in mcpTools)
                        {
                            var json = System.Text.Json.JsonSerializer.Serialize(mcpTool);
                            using var doc = System.Text.Json.JsonDocument.Parse(json);
                            var root = doc.RootElement;

                            ret.Add(new OllamaSharp.Models.Chat.Tool
                            {
                                Function = new OllamaSharp.Models.Chat.Function
                                {
                                    Name = root.TryGetProperty("name", out var n) ? n.GetString() : root.TryGetProperty("Name", out var n2) ? n2.GetString() : null,
                                    Description = root.TryGetProperty("description", out var d) ? d.GetString() : root.TryGetProperty("Description", out var d2) ? d2.GetString() : null,
                                    Parameters = MapMcpInputSchemaToParameters(root.TryGetProperty("inputSchema", out var s) ? s : root.TryGetProperty("InputSchema", out var s2) ? s2 : default)
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tools from MCP servers");
            }

            return ret;
        }

        private OllamaSharp.Models.Chat.Parameters? MapMcpInputSchemaToParameters(System.Text.Json.JsonElement schema)
        {
            if (schema.ValueKind != System.Text.Json.JsonValueKind.Object) return null;

            var parameters = new OllamaSharp.Models.Chat.Parameters();

            if (schema.TryGetProperty("type", out var typeProp))
            {
                parameters.Type = typeProp.GetString();
            }

            if (schema.TryGetProperty("properties", out var propertiesProp) && propertiesProp.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                parameters.Properties = new Dictionary<string, OllamaSharp.Models.Chat.Property>();
                foreach (var prop in propertiesProp.EnumerateObject())
                {
                    var propValue = prop.Value;
                    var property = new OllamaSharp.Models.Chat.Property();
                    if (propValue.TryGetProperty("type", out var pType))
                    {
                        property.Type = pType.GetString();
                    }
                    if (propValue.TryGetProperty("description", out var pDesc))
                    {
                        property.Description = pDesc.GetString();
                    }
                    if (propValue.TryGetProperty("enum", out var pEnum) && pEnum.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        property.Enum = pEnum.EnumerateArray().Select(e => e.GetString()).ToList();
                    }
                    parameters.Properties.Add(prop.Name, property);
                }
            }

            if (schema.TryGetProperty("required", out var requiredProp) && requiredProp.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                parameters.Required = requiredProp.EnumerateArray().Select(r => r.GetString()).ToList();
            }

            return parameters;
        }

        private ThinkValue MapThinkLevel(int level)
        {
            return level switch
            {
                1 => ThinkValue.Low,
                2 => ThinkValue.Medium,
                3 => ThinkValue.High,
                _ => ThinkValue.Low // Fallback for None (0) and Auto (4)
            };
        }
    }
}
