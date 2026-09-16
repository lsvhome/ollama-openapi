using System;
using System.Collections.Generic;
using System.Linq;

namespace OllamaOpenApi.Dto
{
    public class SubmitQueryRequest
    {
        public SubmitQueryRequest() { }
        public SubmitQueryRequest(string prompt, Guid? sessionId)
        {
            Prompt = prompt;
            SessionId = sessionId;
        }
        public string Prompt { get; set; } = string.Empty;
        public Guid? SessionId { get; set; }
    }

    public class QueueSessionsRequest
    {
        public QueueSessionsRequest() { }
        public QueueSessionsRequest(IReadOnlyList<Guid> sessionIds)
        {
            SessionIds = sessionIds?.ToList() ?? new List<Guid>();
        }
        public List<Guid> SessionIds { get; set; } = new();
    }

    public class UpdateSessionSystemPromptRequest
    {
        public UpdateSessionSystemPromptRequest() { }
        public UpdateSessionSystemPromptRequest(string prompt)
        {
            Prompt = prompt;
        }
        public string Prompt { get; set; } = string.Empty;
    }

    public class CreateSessionRequest
    {
        public CreateSessionRequest() { }
        public CreateSessionRequest(string? model, int? systemPromptId, int thinkLevel, string? prompt, string? title, string? systemPromptText, string? workingFolder)
        {
            Model = model;
            SystemPromptId = systemPromptId;
            ThinkLevel = thinkLevel;
            Prompt = prompt;
            Title = title;
            SystemPromptText = systemPromptText;
            WorkingFolder = workingFolder;
        }
        public string? Model { get; set; }
        public int? SystemPromptId { get; set; }
        public int ThinkLevel { get; set; }
        public string? Prompt { get; set; }
        public string? Title { get; set; }
        public string? SystemPromptText { get; set; }
        public string? WorkingFolder { get; set; }
    }

    public class SystemPromptRequest
    {
        public SystemPromptRequest() { }
        public SystemPromptRequest(string text)
        {
            Text = text;
        }
        public string Text { get; set; } = string.Empty;
    }

    public class UpdateUserPromptTextRequest
    {
        public UpdateUserPromptTextRequest() { }
        public UpdateUserPromptTextRequest(string text)
        {
            Text = text;
        }
        public string Text { get; set; } = string.Empty;
    }

    public class UpdateSessionWorkingFolderRequest
    {
        public UpdateSessionWorkingFolderRequest() { }
        public UpdateSessionWorkingFolderRequest(string workingFolder)
        {
            WorkingFolder = workingFolder;
        }
        public string WorkingFolder { get; set; } = string.Empty;
    }

    public class UpdateSessionPriorityRequest
    {
        public UpdateSessionPriorityRequest() { }
        public UpdateSessionPriorityRequest(int priority)
        {
            Priority = priority;
        }
        public int Priority { get; set; }
    }
}
