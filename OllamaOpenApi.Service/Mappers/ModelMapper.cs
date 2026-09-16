using Riok.Mapperly.Abstractions;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Mappers
{
    [Mapper]
    public partial class ModelMapper
    {
        public partial ModelDto ModelInfoToModelDto(ModelInfo modelInfo);
        [MapperIgnoreSource(nameof(ModelDto.SizeHumanReadable))]
        public partial ModelInfo ModelDtoToModelInfo(ModelDto modelDto);
    }
}
