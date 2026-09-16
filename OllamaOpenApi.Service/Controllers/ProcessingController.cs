using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingController : ControllerBase
    {
        private readonly IOllamaAgentService _agentService;

        public ProcessingController(IOllamaAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuery([FromBody] SubmitQueryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "Prompt is required." });
            }

            var sessionId = await _agentService.SubmitQueryAsync(request.Prompt, request.SessionId);
            return CreatedAtRoute("GetSession", new { id = sessionId }, new { sessionId });
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessSessions([FromBody] QueueSessionsRequest request)
        {
            if (request == null || request.SessionIds == null || request.SessionIds.Count == 0)
            {
                return BadRequest(new { error = "At least one session ID is required." });
            }

            await _agentService.AddToExecutionListAsync(new List<Guid>(request.SessionIds));
            return Accepted();
        }

        [HttpPost("pause")]
        public async Task<IActionResult> Pause()
        {
            await _agentService.PauseExecutionListAsync();
            return NoContent();
        }

        [HttpPost("resume")]
        public async Task<IActionResult> Resume()
        {
            await _agentService.ResumeExecutionListAsync();
            return NoContent();
        }

        [HttpPost("interrupt")]
        public async Task<IActionResult> Interrupt()
        {
            await _agentService.InterruptActiveSessionAsync();
            return NoContent();
        }

        [HttpGet("activeSession")]
        public async Task<IActionResult> GetActiveSession()
        {
            var session = await _agentService.GetActiveProcessingSessionAsync();
            if (session == null)
            {
                return NotFound(new { message = "No active session" });
            }
            return Ok(session);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var executionList = _agentService.GetExecutionList();
            return Ok(new { status = executionList.Status });
        }
    }
}
