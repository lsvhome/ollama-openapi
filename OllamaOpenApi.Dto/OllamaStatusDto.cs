namespace OllamaOpenApi.Dto
{
    public sealed record OllamaStatusDto(
        string? Name,
        string? Model,
        long Size,
        string? Digest,
        OllamaStatusDetails? Details,
        long Size_Vram)
    {
        public int Gpu => Size == 0 ? 0 : (int)((double)Size_Vram / (double)Size * 100.0);

        public string SizeHumanReadable
        {
            get
            {
                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{(Size / 1024)} Kb";
                if (Size < 1024 * 1024 * 1024) return $"{(Size / (1024 * 1024))} Mb";
                return $"{(double)Size / (1024 * 1024 * 1024):0.0} Gb";
            }
        }
    }

    public sealed record OllamaStatusDetails(
        string? ParentModel,
        string? Format,
        string? Family,
        string? ParameterSize,
        string? QuantizationLevel);

    public sealed record OllamaStatusResponseDto(
        IReadOnlyList<OllamaStatusDto>? Models);
}
