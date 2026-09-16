namespace OllamaOpenApi.Dto
{
    public class McpServerConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "http";
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string>? Env { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }

    public class McpGlobalConfig
    {
        public Dictionary<string, McpServerConfig> McpServers { get; set; } = new();
    }
}
