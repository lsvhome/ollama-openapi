using System;
using System.Collections.Generic;

namespace OllamaOpenApi.Dto
{
    public class ModelDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public string? ModelName { get; set; }
        public DateTime ModifiedAt { get; set; }
        public long Size { get; set; }
        public string Digest { get; set; } = string.Empty;
        public ModelDtoDetails? Details { get; set; }

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

        public static ModelDto FromModel(ModelInfo model) => new()
        {
            Name = model.Name,
            IsDefault = model.IsDefault,
            ModelName = model.ModelName,
            ModifiedAt = model.ModifiedAt,
            Size = model.Size,
            Digest = model.Digest,
            Details = ModelDtoDetails.FromDetails(model.Details)
        };
    }

    public class ModelDtoDetails
    {
        public string? ParentModel { get; set; }
        public string Format { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string[]? Families { get; set; }
        public string ParameterSize { get; set; } = string.Empty;
        public string QuantizationLevel { get; set; } = string.Empty;

        public static ModelDtoDetails FromDetails(ModelDetails details) => new()
        {
            ParentModel = details.ParentModel,
            Format = details.Format,
            Family = details.Family,
            Families = details.Families,
            ParameterSize = details.ParameterSize,
            QuantizationLevel = details.QuantizationLevel
        };
    }

    public record SetModelRequest(string Name);
}
