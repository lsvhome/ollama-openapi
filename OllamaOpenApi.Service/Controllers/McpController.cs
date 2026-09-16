using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OllamaOpenApi.Service.Services;
using System.Net;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/mcp")]
    public class McpController : ControllerBase
    {
        private readonly IMcpConfigService _configService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<McpController> _logger;

        public McpController(IMcpConfigService configService, IHttpClientFactory httpClientFactory, ILogger<McpController> logger)
        {
            _configService = configService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("check/{serverName}")]
        public async Task<IActionResult> CheckServerAvailability(string serverName, [FromQuery] Guid? sessionId = null)
        {
            var server = await _configService.GetServerAsync(serverName, sessionId);
            if (server == null)
            {
                return NotFound(new { Message = $"MCP server '{serverName}' not found in global or session configuration." });
            }

            try
            {
                var clientTransport = new HttpClientTransport(new()
                {
                    Endpoint = new Uri(server.Url)
                });

                await using (var mcpClient = await McpClient.CreateAsync(clientTransport!))
                {
                    var tools = await mcpClient.ListToolsAsync();
                    if (tools.Any())
                    {
                        return Ok(new { Server = serverName, Available = true, Tools = tools });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability of MCP server {ServerName} at {Url}", serverName, server.Url);
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Server = serverName, Available = false, Error = ex.Message });
            }

            return StatusCode(404, new { Server = serverName, Available = false, StatusCode = 404 });
        }

        [HttpGet("servers")]
        public async Task<IActionResult> ListServers([FromQuery] Guid? sessionId = null)
        {
            var servers = await _configService.GetServersAsync(sessionId);
            return Ok(servers);
        }
    }
}
