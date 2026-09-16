using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Service.Services;
using System.Threading.Tasks;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/ollama")]
    public class OllamaController : ControllerBase
    {
        private readonly IOllamaAgentService _ollamaAgentService;

        public OllamaController(IOllamaAgentService ollamaAgentService)
        {
            _ollamaAgentService = ollamaAgentService;
        }

        [HttpGet("ps")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _ollamaAgentService.GetOllamaStatusAsync();
                return Content(status, "application/json");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("load/{model}")]
        public async Task<IActionResult> LoadModel(string model)
        {
            try
            {
                await _ollamaAgentService.LoadModelAsync(model);
                return Ok($"Model {model} loaded successfully.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("unload/{model}")]
        public async Task<IActionResult> UnloadModel(string model)
        {
            try
            {
                await _ollamaAgentService.UnloadModelAsync(model);
                return Ok($"Model {model} unloaded successfully.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("reload/{model}")]
        public async Task<IActionResult> ReloadModel(string model)
        {
            try
            {
                await _ollamaAgentService.ReloadModelAsync(model);
                return Ok($"Model {model} reloaded successfully.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
