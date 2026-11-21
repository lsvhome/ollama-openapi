using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services
{
    public partial class OllamaAgentService
    {
        // Model Management
        public Task<List<ModelInfo>> GetModelsAsync() => Task.FromResult(_models);
        public Task<ModelInfo?> GetDefaultModelAsync() => Task.FromResult(_models.FirstOrDefault(m => m.IsDefault));
        public Task SetDefaultModelAsync(string modelName)
        {
            foreach (var m in _models) m.IsDefault = (m.Name == modelName);
            return Task.CompletedTask;
        }

        private async Task InitModels()
        {
            var models = await _ollamaClient!.ListLocalModelsAsync();
            _logger.LogInformation("Connected to Ollama and loaded {Count} models:", models.Count());

            _models.Clear();
            foreach (var m in models.OrderBy(m => m.Name.ToLowerInvariant()))
            {
                _models.Add(new ModelInfo { Name = m.Name, Size = m.Size, IsDefault = false, Details = new ModelDetails { } });
            }
        }

        private async Task LoadDefaultModelAsync()
        {
            await InitModels();

            // Default Model
            string defaultModelName = _configuration["OLLAMA_MODEL"] ?? throw new ApplicationException("Environment variable OLLAMA_MODEL not configured");

            _logger.LogInformation("Connected to Ollama and loaded {Count} models: {@Models}", _models.Count, _models.Select(m => m.Name));

            // Check Default Model found
            var defaultModel = _models.FirstOrDefault(m => m.Name == defaultModelName);
            if (defaultModel == null && _models.Any())
            {
                defaultModel = _models.OrderBy(m => m.Size).FirstOrDefault();
                _logger.LogInformation("Configured default model {ModelName} not found or not specified. Falling back to smallest model: {SmallestModel}", defaultModelName, defaultModel?.Name);
            }
            else if (defaultModel != null)
            {
                _logger.LogInformation("Using configured default model: {ModelName}", defaultModelName);
            }

            // Load Default Model
            if (defaultModel != null)
            {
                defaultModel.IsDefault = true;
                try
                {
                    _logger.LogInformation("Loading default model {ModelName}...", defaultModel.Name);
                    await LoadModelAsync(defaultModel.Name);
                    _ollamaClient!.SelectedModel = defaultModelName!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load default model {ModelName}", defaultModel.Name);

                    await Task.Delay(TimeSpan.FromSeconds(10));

                    try
                    {
                        _logger.LogInformation("Second try to load default model {ModelName}...", defaultModel.Name);
                        await LoadModelAsync(defaultModel.Name);
                        _ollamaClient!.SelectedModel = defaultModelName!;
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogError(ex2, "Failed second time to load default model {ModelName}", defaultModel.Name);
                    }
                }
            }
        }
    }
}
