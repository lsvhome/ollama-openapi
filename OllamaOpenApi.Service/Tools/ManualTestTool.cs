using OllamaSharp.Models.Chat;

namespace OllamaOpenApi.Service.Tools;

public class ManualTestTool : Tool
{
    public ManualTestTool()
    {
        this.Function = new Function
        {
            Name = "ManualTestTool",
            Description = "Test Tool"
        };
    }
}
