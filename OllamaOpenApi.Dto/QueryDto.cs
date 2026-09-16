using System;
using System.Linq;
using System.Collections.Generic;
using OllamaSharp.Models.Chat;

namespace OllamaOpenApi.Dto
{
    public class MessageDto
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string? Thinking { get; set; }
        public IEnumerable<string>? ToolCalls { get; set; }
        public string? ToolName { get; set; }

        public static MessageDto FromMessage(Message msg) => new()
        {
            Role = msg.Role?.ToString() ?? string.Empty,
            Content = msg.Content ?? string.Empty,
            Created = msg.Created,
            Thinking = msg.Thinking,
            ToolCalls = msg.ToolCalls?.Select(tc => tc.Function?.Name ?? string.Empty).ToList(),
            ToolName = msg.ToolName
        };
    }

    public class UserPromptDto
    {
        public Guid Id { get; set; }
        public string UserPromptText { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public List<MessageDto> Messages { get; set; } = new();
        public DateTime CreatedTimestamp { get; set; }
        public DateTime? SentTimestamp { get; set; }
        public DateTime? CompletedTimestamp { get; set; }
        public UserPromptStatus Status { get; set; }

        public static UserPromptDto FromQuery(UserPrompt query) => new()
        {
            Id = query.Id,
            UserPromptText = query.UserPromptText,
            Response = query.Response,
            Messages = query.Messages.Select(MessageDto.FromMessage).ToList(),
            CreatedTimestamp = query.CreatedTimestamp,
            SentTimestamp = query.SentTimestamp,
            CompletedTimestamp = query.CompletedTimestamp,
            Status = query.Status
        };
    }
}
