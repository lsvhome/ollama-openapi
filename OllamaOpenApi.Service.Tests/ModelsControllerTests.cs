using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Tests
{
    public class ModelsControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task GetModels_ReturnsOk_AndEmptyListInitially()
        {
            var response = await Client.GetAsync("/api/models/list");
            response.EnsureSuccessStatusCode();

            var models = await response.Content.ReadFromJsonAsync<System.Collections.Generic.IReadOnlyList<ModelDto>>();
            Assert.NotNull(models);
            Assert.Empty(models);
        }

        [Fact]
        public async Task GetDefaultModel_ReturnsNotFound_WhenNoDefault()
        {
            var response = await Client.GetAsync("/api/models/default");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SetDefaultModel_WithEmptyName_ReturnsBadRequest()
        {
            var response = await Client.PutAsJsonAsync("/api/models/default", new SetModelRequest(""));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SetDefaultModel_WithValidName_ReturnsNoContent()
        {
            var response = await Client.PutAsJsonAsync("/api/models/default", new SetModelRequest("llama3"));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
