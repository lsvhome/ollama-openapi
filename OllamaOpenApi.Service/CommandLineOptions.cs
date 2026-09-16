using CommandLine;

namespace OllamaOpenApi.Service
{
    public class CommandLineOptions
    {
        [Option('m', "model", Required = false, HelpText = "The default model to use.")]
        public string? Model { get; set; }

        [Option('u', "urls", Required = false, HelpText = "Listening url override.")]
        public string? Urls { get; set; }
    }
}
