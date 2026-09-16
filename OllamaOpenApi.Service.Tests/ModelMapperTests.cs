using System;
using Xunit;
using OllamaOpenApi.Service.Mappers;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Tests
{
    public class ModelMapperTests
    {
        private readonly ModelMapper _mapper = new();

        [Fact]
        public void ModelInfoToModelDto_ShouldMapCorrectly()
        {
            // Arrange
            var modelInfo = new ModelInfo
            {
                Name = "test-model",
                IsDefault = true,
                ModelName = "test-model-id",
                ModifiedAt = DateTime.UtcNow,
                Size = 12345,
                Digest = "hash123",
                Details = new ModelDetails
                {
                    ParentModel = "parent",
                    Format = "gguf",
                    Family = "llama",
                    Families = new[] { "llama", "meta" },
                    ParameterSize = "8B",
                    QuantizationLevel = "q4_K_M"
                }
            };

            // Act
            var result = _mapper.ModelInfoToModelDto(modelInfo);

            // Assert
            Assert.Equal(modelInfo.Name, result.Name);
            Assert.Equal(modelInfo.IsDefault, result.IsDefault);
            Assert.Equal(modelInfo.ModelName, result.ModelName);
            Assert.Equal(modelInfo.ModifiedAt, result.ModifiedAt);
            Assert.Equal(modelInfo.Size, result.Size);
            Assert.Equal(modelInfo.Digest, result.Digest);
            Assert.Equal(modelInfo.Details.ParentModel, result.Details.ParentModel);
            Assert.Equal(modelInfo.Details.Format, result.Details.Format);
            Assert.Equal(modelInfo.Details.Family, result.Details.Family);
            Assert.Equal(modelInfo.Details.Families, result.Details.Families);
            Assert.Equal(modelInfo.Details.ParameterSize, result.Details.ParameterSize);
            Assert.Equal(modelInfo.Details.QuantizationLevel, result.Details.QuantizationLevel);
        }

        [Fact]
        public void ModelDtoToModelInfo_ShouldMapCorrectly()
        {
            // Arrange
            var modelDto = new ModelDto
            {
                Name = "test-model",
                IsDefault = true,
                ModelName = "test-model-id",
                ModifiedAt = DateTime.UtcNow,
                Size = 12345,
                Digest = "hash123",
                Details = new ModelDtoDetails
                {
                    ParentModel = "parent",
                    Format = "gguf",
                    Family = "llama",
                    Families = new[] { "llama", "meta" },
                    ParameterSize = "8B",
                    QuantizationLevel = "q4_K_M"
                }
            };

            // Act
            var result = _mapper.ModelDtoToModelInfo(modelDto);

            // Assert
            Assert.Equal(modelDto.Name, result.Name);
            Assert.Equal(modelDto.IsDefault, result.IsDefault);
            Assert.Equal(modelDto.ModelName, result.ModelName);
            Assert.Equal(modelDto.ModifiedAt, result.ModifiedAt);
            Assert.Equal(modelDto.Size, result.Size);
            Assert.Equal(modelDto.Digest, result.Digest);
            Assert.Equal(modelDto.Details.ParentModel, result.Details.ParentModel);
            Assert.Equal(modelDto.Details.Format, result.Details.Format);
            Assert.Equal(modelDto.Details.Family, result.Details.Family);
            Assert.Equal(modelDto.Details.Families, result.Details.Families);
            Assert.Equal(modelDto.Details.ParameterSize, result.Details.ParameterSize);
            Assert.Equal(modelDto.Details.QuantizationLevel, result.Details.QuantizationLevel);
        }
    }
}
