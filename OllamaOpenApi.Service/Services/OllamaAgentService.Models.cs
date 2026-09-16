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
    }
}
