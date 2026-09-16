using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/models")]
    public sealed class ModelsController : ControllerBase
    {
        private readonly IOllamaAgentService _agent;

        public ModelsController(IOllamaAgentService agent)
        {
            _agent = agent;
        }

        [HttpGet("list")]
        public async Task<ActionResult<IReadOnlyList<ModelDto>>> GetModels()
        {
            var models = await _agent.GetModelsAsync();
            return Ok(models.Select(ModelDto.FromModel).ToList());
        }

        [HttpGet("default")]
        public async Task<ActionResult<ModelDto>> GetDefaultModel()
        {
            var model = await _agent.GetDefaultModelAsync();
            return model is null ? NotFound() : Ok(ModelDto.FromModel(model));
        }

        [HttpPut("default")]
        public async Task<IActionResult> SetDefaultModel([FromBody] SetModelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest(new { error = "Model name is required." });

            await _agent.SetDefaultModelAsync(request.Name);
            return NoContent();
        }
    }
}
