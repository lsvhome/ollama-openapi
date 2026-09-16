using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        public async Task<string> GetOllamaStatusAsync()
        {
            var host = _configuration["ollama_host"];
            if (string.IsNullOrEmpty(host))
            {
                throw new InvalidOperationException("Configuration key 'ollama_host' is missing.");
            }

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            try
            {
                var response = await httpClient.GetStringAsync($"{host}/api/ps");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Ollama status from {Host}", host);
                throw;
            }
        }
    }
}
