using System;
using System.Collections.Generic;
using System.Linq;

namespace OllamaOpenApi.Dto
{
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public bool Archived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Model { get; set; }
        public int ThinkLevel { get; set; } = 0;
        public int? SystemPromptId { get; set; }
        public string SystemPrompt { get; set; } = string.Empty;
        public string WorkingFolder { get; set; } = string.Empty;
        public List<UserPrompt> UserPrompts { get; set; } = new List<UserPrompt>();
        public List<McpServerConfig> McpServers { get; set; } = new List<McpServerConfig>();
        public bool HasQueriesToProcess() => UserPrompts.Any(up => up.Status == UserPromptStatus.Ready || up.Status == UserPromptStatus.InProgress);

        public UserPromptStatus Status
        {
            get
            {
                if (this.UserPrompts.Any(up => up.Status == UserPromptStatus.InProgress))
                {
                    return UserPromptStatus.InProgress;
                }

                if (this.UserPrompts.Any(up => up.Status == UserPromptStatus.Error))
                {
                    return UserPromptStatus.Error;
                }

                if (this.UserPrompts.Any(up => up.Status == UserPromptStatus.Ready))
                {
                    return UserPromptStatus.Ready;
                }

                if (this.UserPrompts.Any(up => up.Status == UserPromptStatus.Draft))
                {
                    return UserPromptStatus.Draft;
                }

                if (this.UserPrompts.All(up => up.Status == UserPromptStatus.Completed))
                {
                    return UserPromptStatus.Completed;
                }

                return UserPromptStatus.Draft;
            }
        }
    }
}
