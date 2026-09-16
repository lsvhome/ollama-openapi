using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/folders")]
    public sealed class FoldersController : ControllerBase
    {
        [HttpGet("list")]
        public IActionResult ListFolders([FromQuery] string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            try
            {
                if (!Directory.Exists(path))
                {
                    return NotFound(new { error = "Directory not found." });
                }

                var directories = Directory.GetDirectories(path)
                    .Select(d => new FolderDto(Path.GetFileName(d) ?? d, d, true))
                    .ToList();

                var files = Directory.GetFiles(path)
                    .Select(f => new FolderDto(Path.GetFileName(f) ?? f, f, false))
                    .ToList();

                return Ok(directories.Concat(files).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
