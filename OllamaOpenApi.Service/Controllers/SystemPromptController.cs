using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemPromptController : ControllerBase
    {
        private readonly IOllamaAgentService _agentService;

        public SystemPromptController(IOllamaAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<SystemPrompt>>> GetAll()
        {
            var prompts = await _agentService.GetSystemPromptsAsync();
            return Ok(prompts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SystemPrompt>> GetById(int id)
        {
            var prompt = (await _agentService.GetSystemPromptsAsync()).FirstOrDefault(p => p.Id == id);
            if (prompt == null) return NotFound();
            return Ok(prompt);
        }

        [HttpPut]
        public async Task<ActionResult<SystemPrompt>> Create([FromBody] SystemPromptRequest request)
        {
            var prompt = new SystemPrompt { Text = request.Text };
            await _agentService.AddSystemPromptAsync(prompt);
            return CreatedAtAction(nameof(GetById), new { id = prompt.Id }, prompt);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SystemPromptRequest request)
        {
            var prompt = new SystemPrompt { Id = id, Text = request.Text };
            await _agentService.UpdateSystemPromptAsync(prompt);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _agentService.DeleteSystemPromptAsync(id);
            return NoContent();
        }
    }
}
