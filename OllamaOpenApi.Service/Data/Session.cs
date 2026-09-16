namespace OllamaOpenApi.Service.Data;

public class SessionDbEntity
{
    public Guid Id { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public string? Metadata { get; set; }
}
