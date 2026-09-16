using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Services;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public sealed class SessionsController : ControllerBase
    {
        private readonly IOllamaAgentService _agent;

        public SessionsController(IOllamaAgentService agent)
        {
            _agent = agent;
        }

        [HttpGet("list")]
        public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetSessions()
        {
            var sessions = await _agent.ListSessionsAsync();
            var result = new List<SessionDto>(sessions.Count);
            foreach (var session in sessions) result.Add(SessionDto.FromSession(session));
            return Ok(result);
        }

        [HttpGet("{id:guid}", Name = "GetSession")]
        public async Task<IActionResult> GetSession(Guid id)
        {
            var sessions = await _agent.ListSessionsAsync();
            var session = sessions.Find(s => s.Id == id);
            if (session == null) return NotFound();
            return Ok(SessionDto.FromSession(session));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            var sessionId = await _agent.CreateSessionAsync(request.Model, request.SystemPromptId, request.ThinkLevel, request.Prompt, request.Title, request.SystemPromptText, request.WorkingFolder);
            return Ok(new { sessionId });
        }

        [HttpPut("{id:guid}/title")]
        public async Task<IActionResult> UpdateSessionTitle(Guid id, [FromBody] UpdateSessionTitleRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { error = "Title is required." });

            await _agent.UpdateSessionTitleAsync(id, request.Title);
            return NoContent();
        }

        [HttpPut("{id:guid}/working-folder")]
        public async Task<IActionResult> UpdateSessionWorkingFolder(Guid id, [FromBody] UpdateSessionWorkingFolderRequest request)
        {
            if (request == null || request.WorkingFolder == null)
                return BadRequest(new { error = "Working folder is required." });

            await _agent.UpdateSessionWorkingFolderAsync(id, request.WorkingFolder);
            return NoContent();
        }

        [HttpGet("{id:guid}/export")]
        public async Task<IActionResult> ExportSession(Guid id)
        {
            try
            {
                var data = await _agent.ExportSessionAsync(id);
                return File(data, "application/json", $"{id}.session.json");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id:guid}/import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportSession(Guid id, IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "Payload is empty." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            await _agent.ImportSessionAsync(ms.ToArray());
            return NoContent();
        }

        [HttpGet("export-all")]
        public async Task<IActionResult> ExportAllSessions()
        {
            var data = await _agent.ExportAllSessionsAsync();
            return File(data, "application/json", "sessions-export.json");
        }

        [HttpPost("import-all")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportAllSessions(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { error = "Payload is empty." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            await _agent.ImportAllSessionsAsync(ms.ToArray());
            return NoContent();
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveSessions()
        {
            await _agent.SaveSessionsToDiskAsync();
            return Ok(new { message = "Sessions saved to disk." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSession(Guid id)
        {
            await _agent.DeleteSessionAsync(id);
            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAllSessions()
        {
            await _agent.DeleteAllSessionsAsync();
            return NoContent();
        }

        [HttpPut("{id:guid}/archive")]
        public async Task<IActionResult> ArchiveSession(Guid id)
        {
            await _agent.ArchiveSessionAsync(id, true);
            await _agent.SaveSessionsToDiskAsync();
            return NoContent();
        }

        [HttpPut("{id:guid}/unarchive")]
        public async Task<IActionResult> UnarchiveSession(Guid id)
        {
            await _agent.ArchiveSessionAsync(id, false);
            await _agent.SaveSessionsToDiskAsync();
            return NoContent();
        }

        [HttpPut("{id:guid}/priority")]
        public async Task<IActionResult> UpdateSessionPriority(Guid id, [FromBody] UpdateSessionPriorityRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            await _agent.UpdateSessionPriorityAsync(id, request.Priority);
            return NoContent();
        }
    }

    public sealed record UpdateSessionTitleRequest(string Title);
}
