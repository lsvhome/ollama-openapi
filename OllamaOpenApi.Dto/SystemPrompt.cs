using System;

namespace OllamaOpenApi.Dto
{
    public class SystemPrompt
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsReadOnly { get; set; }
    }
}
