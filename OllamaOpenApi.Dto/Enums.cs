namespace OllamaOpenApi.Dto
{
    public enum UserPromptStatus
    {
        Draft,
        Ready,
        InProgress,
        Completed,
        Error
    }

    public enum ExecutionListStatus
    {
        Paused = 0,
        Pausing = 1,
        InProgress = 2
    }

    public enum ExternalSourceType
    {
        Default = 0,
        Email = 1
    }
}
