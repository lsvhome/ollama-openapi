using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Services;

public class ExternalSourceEmail : IExternalSource
{
    private ILogger<ExternalSourceEmail> _logger;

    public ExternalSourceEmail(ILogger<ExternalSourceEmail> logger) : base()
    {
        _logger = logger;
    }

    public bool CanProceess(ExternalSourceType externalSourceType)
    {
        return ExternalSourceType.Email == externalSourceType;
    }

    public async Task Process(Session session)
    {
        await Task.CompletedTask;
    }
}