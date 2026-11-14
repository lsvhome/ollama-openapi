namespace OllamaOpenApi.Dto
{
    public interface IExternalSource
    {
        bool CanProceess(ExternalSourceType externalSourceType);
        Task Process(Session session);
    }
}