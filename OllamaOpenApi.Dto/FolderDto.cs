namespace OllamaOpenApi.Dto
{
    public class FolderDto
    {
        public FolderDto() { }
        public FolderDto(string name, string path, bool isDirectory)
        {
            Name = name;
            Path = path;
            IsDirectory = isDirectory;
        }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
    }
}
