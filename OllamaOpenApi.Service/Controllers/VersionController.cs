using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace OllamaOpenApi.Service.Controllers
{
    [ApiController]
    [Route("api/version")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString();
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            return Ok(new
            {
                AssemblyVersion = version,
                AssemblyInformationalVersion = informationalVersion
            });
        }
    }
}
