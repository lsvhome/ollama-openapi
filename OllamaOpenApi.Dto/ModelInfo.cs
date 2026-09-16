using System.Diagnostics;
using System.Text.Json.Serialization;
using Riok.Mapperly.Abstractions;

namespace OllamaOpenApi.Dto
{
    [DebuggerDisplay("{Name}")]
    public class ModelInfo
    {
        public bool IsDefault { get; set; }

        //
        // Summary:
        //     Gets or sets the name of the model.
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        //
        // Summary:
        //     Gets or sets the model identifier. The Ollama API returns both name and model.
        [JsonPropertyName("model")]
        public string? ModelName { get; set; }

        //
        // Summary:
        //     Gets or sets the time the model was created or last modified.
        [JsonPropertyName("modified_at")]
        public DateTime ModifiedAt { get; set; }

        //
        // Summary:
        //     Gets or sets the size of the model file in bytes.
        [JsonPropertyName("size")]
        public long Size { get; set; }

        //
        // Summary:
        //     Gets or sets a cryptographic hash of the model file.
        [JsonPropertyName("digest")]
        public string Digest { get; set; } = string.Empty;

        //
        // Summary:
        //     Gets or sets additional details about the model.
        [JsonPropertyName("details")]
        public ModelDetails Details { get; set; } = null!;

    }

    public class ModelDetails
    {
        /// <summary>
        /// Gets or sets the name of the parent model on which the model is based.
        /// </summary>
        // [JsonPropertyName(Application.ParentModel)]
        public string? ParentModel { get; set; }

        /// <summary>
        /// Gets or sets the format of the model file.
        /// </summary>
        // [JsonPropertyName(Application.Format)]
        public string Format { get; set; } = null!;

        /// <summary>
        /// Gets or sets the family of the model.
        /// </summary>
        // [JsonPropertyName(Application.Family)]
        public string Family { get; set; } = null!;

        /// <summary>
        /// Gets or sets the families of the model.
        /// </summary>
        // [JsonPropertyName(Application.Families)]
        public string[]? Families { get; set; }

        /// <summary>
        /// Gets or sets the number of parameters in the model.
        /// </summary>
        // [JsonPropertyName(Application.ParameterSize)]
        public string ParameterSize { get; set; } = null!;

        /// <summary>
        /// Gets or sets the quantization level of the model.
        /// </summary>
        // [JsonPropertyName(Application.QuantizationLevel)]
        public string QuantizationLevel { get; set; } = null!;
    }

}
