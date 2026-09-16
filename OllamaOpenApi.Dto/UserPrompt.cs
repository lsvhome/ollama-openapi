using System;
using System.Collections.Generic;
using System.Linq;
using OllamaSharp.Models.Chat;

namespace OllamaOpenApi.Dto
{
    public class UserPrompt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserPromptText { get; set; } = string.Empty;
        public List<Message> Messages { get; set; } = new List<Message>();
        public List<Message> Thoughts { get; set; } = new List<Message>();
        public string Response { get; set; } = string.Empty;
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
        public DateTime? SentTimestamp { get; set; }
        public DateTime? CompletedTimestamp { get; set; }
        public UserPromptStatus Status { get; set; } = UserPromptStatus.Draft;
    }
}
