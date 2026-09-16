using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        public async Task LoadModelAsync(string model)
        {
            var host = _configuration["ollama_host"];
            if (string.IsNullOrEmpty(host)) throw new InvalidOperationException("Configuration key 'ollama_host' is missing.");

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            var requestBody = new { model = model, prompt = "", keep_alive = -1 };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{host}/api/generate", content);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Model {Model} loaded successfully.", model);
        }

        public async Task UnloadModelAsync(string model)
        {
            var host = _configuration["ollama_host"];
            if (string.IsNullOrEmpty(host)) throw new InvalidOperationException("Configuration key 'ollama_host' is missing.");

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            var requestBody = new { model = model, prompt = "", keep_alive = 0 };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{host}/api/generate", content);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Model {Model} unloaded successfully.", model);
        }

        public async Task ReloadModelAsync(string model)
        {
            _logger.LogInformation("Reloading model {Model}...", model);

            // 1. Unload model
            await UnloadModelAsync(model);

            // 2. Compact memory
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"sudo sync && echo 1 | sudo tee /proc/sys/vm/compact_memory\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                await process!.WaitForExitAsync();
                _logger.LogInformation("Memory compaction executed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute memory compaction script.");
                // We continue as this might be a permission issue but we still want to try loading the model
            }

            // 3. Load model
            await LoadModelAsync(model);

            // 4. Wait while model loaded
            bool isLoaded = false;
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(2000);
                var statusJson = await GetOllamaStatusAsync();
                if (statusJson.Contains($"\"model\":\"{model}"))
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded)
            {
                throw new Exception($"Model {model} failed to load within timeout.");
            }

            // 5. Refresh ollama status and check VRAM
            var finalStatusJson = await GetOllamaStatusAsync();
            var status = JsonSerializer.Deserialize<OllamaPsResponse>(finalStatusJson);
            var modelStatus = status?.Models?.FirstOrDefault(m => m.Model == model);

            var vramConfig = _configuration["size_vram"];
            if (long.TryParse(vramConfig, out long sizeVram))
            {
                if (modelStatus == null || modelStatus.Size != sizeVram)
                {
                    throw new Exception("Not enough amount of VRAM");
                }
            }
            else
            {
                _logger.LogWarning("size_vram configuration is missing or invalid. Skipping VRAM check.");
            }

            _logger.LogInformation("Model {Model} reloaded successfully and VRAM check passed.", model);
        }

        private class OllamaPsResponse
        {
            public List<OllamaPsModel> Models { get; set; } = new();
        }

        private class OllamaPsModel
        {
            public string Model { get; set; } = string.Empty;
            public long Size { get; set; }
        }
    }
}
