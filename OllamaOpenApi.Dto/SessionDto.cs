using System;
using System.Collections.Generic;
using System.Linq;

namespace OllamaOpenApi.Dto
{
    public class SessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
        public string WorkingFolder { get; set; } = string.Empty;
        public List<UserPromptDto> UserPrompts { get; set; } = new();
        public UserPromptStatus Status { get; init; }
        public bool Archived { get; init; } = false;
        public int Priority { get; init; } = 0;

        public static SessionDto FromSession(Session session) => new()
        {
            Id = session.Id,
            Title = session.Title,
            Archived = session.Archived,
            Priority = session.Priority,
            SystemPrompt = session.SystemPrompt,
            WorkingFolder = session.WorkingFolder,
            Status = session.Status,
            UserPrompts = session.UserPrompts.Select(UserPromptDto.FromQuery).ToList()
        };
    }
}
