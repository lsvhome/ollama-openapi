using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/user-prompts")]
    public sealed class UserPromptController : ControllerBase
    {
        private readonly IOllamaAgentService _agent;

        public UserPromptController(IOllamaAgentService agent)
        {
            _agent = agent;
        }

        [HttpPost("{sessionId:guid}/text")]
        public async Task<IActionResult> Create(Guid sessionId, [FromBody] UpdateUserPromptTextRequest request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            var success = await _agent.AddUserPromptAsync(sessionId, request.Text);
            if (!success) return BadRequest(new { error = "Could not create prompt. Session not found." });

            return Ok();
        }

        [HttpPut("{sessionId:guid}/{promptId:guid}/text")]
        public async Task<IActionResult> UpdateText(Guid sessionId, Guid promptId, [FromBody] UpdateUserPromptTextRequest request)
        {
            if (request == null) return BadRequest(new { error = "Request body is required." });

            var success = await _agent.UpdateUserPromptTextAsync(sessionId, promptId, request.Text);
            if (!success) return BadRequest(new { error = "Could not update prompt text. It must be in 'Draft' status." });

            return NoContent();
        }

        [HttpPatch("{sessionId:guid}/{promptId:guid}/status/{status}")]
        public async Task<IActionResult> UpdateStatus(Guid sessionId, Guid promptId, UserPromptStatus status)
        {
            var success = await _agent.SetUserPromptStatusAsync(sessionId, promptId, status);
            if (!success) return BadRequest(new { error = "Could not update prompt status. Ensure the transition is valid and follows sequential constraints." });

            return NoContent();
        }

        [HttpDelete("{sessionId:guid}/{promptId:guid}")]
        public async Task<IActionResult> Delete(Guid sessionId, Guid promptId)
        {
            var success = await _agent.DeleteUserPromptAsync(sessionId, promptId);
            if (!success) return BadRequest(new { error = "Could not delete prompt. It must be in 'Draft' status." });

            return NoContent();
        }
    }
}
