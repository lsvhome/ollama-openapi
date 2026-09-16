using System.Text.Json;
using OllamaOpenApi.Dto;
using Microsoft.Extensions.Logging;

namespace OllamaOpenApi.Service.Services
{
    public interface IMcpConfigService
    {
        Task<IEnumerable<McpServerConfig>> GetServersAsync(Guid? sessionId = null);
        Task<McpServerConfig?> GetServerAsync(string name, Guid? sessionId = null);
    }

    public class McpConfigService : IMcpConfigService
    {
        private readonly ILogger<McpConfigService> _logger;
        private readonly string _globalConfigPath;
        private readonly OllamaAgentService _agentService;

        public McpConfigService(ILogger<McpConfigService> logger, OllamaAgentService agentService)
        {
            _logger = logger;
            _agentService = agentService;
            _globalConfigPath = Path.Combine(AppContext.BaseDirectory, "mcp.json");
        }

        public async Task<IEnumerable<McpServerConfig>> GetServersAsync(Guid? sessionId = null)
        {
            var servers = new List<McpServerConfig>();

            // Global servers
            var globalConfig = await LoadGlobalConfigAsync();
            if (globalConfig != null)
            {
                servers.AddRange(globalConfig.McpServers.Values);
            }

            // Session servers
            if (sessionId.HasValue)
            {
                var session = await _agentService.GetSessionAsync(sessionId.Value);
                if (session != null && session.McpServers != null)
                {
                    servers.AddRange(session.McpServers);
                }
            }

            return servers.DistinctBy(s => s.Name);
        }

        public async Task<McpServerConfig?> GetServerAsync(string name, Guid? sessionId = null)
        {
            var servers = await GetServersAsync(sessionId);
            return servers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<McpGlobalConfig?> LoadGlobalConfigAsync()
        {
            if (!File.Exists(_globalConfigPath))
            {
                var exampleMcpServeronfigNPath = _globalConfigPath + ".example";
                if (!File.Exists(exampleMcpServeronfigNPath))
                {
                    var examole001 = new McpServerConfig
                    {
                        Name = "ExampleMcpServerame",
                        Type = "http",
                        Url = "http://test.com",
                        Env = new Dictionary<string, string>
                        {
                            {"ENVVARIABLENAME", "envVariableValue"}
                        },
                        Headers = new Dictionary<string, string>
                        {
                            {"Authorization", "Bearer token"}
                        }

                    };

                    var exampleMcpConfig = new McpGlobalConfig
                    {
                        McpServers = new Dictionary<string, McpServerConfig>
                        {
                            {
                                examole001.Name,
                                examole001
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize<McpGlobalConfig>(exampleMcpConfig, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    await File.WriteAllTextAsync(_globalConfigPath + ".example", json);
                }

                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_globalConfigPath);
                return JsonSerializer.Deserialize<McpGlobalConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading global MCP configuration from {Path}", _globalConfigPath);
                return null;
            }
        }
    }
}
