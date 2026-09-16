using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OllamaOpenApi.Service.Tests
{
    public class OllamaControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task GetStatus_ReturnsOk()
        {
            var response = await Client.GetAsync("/api/ollama/ps");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LoadModel_ReturnsOk()
        {
            var response = await Client.PostAsync("/api/ollama/load/llama3", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UnloadModel_ReturnsOk()
        {
            var response = await Client.PostAsync("/api/ollama/unload/llama3", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ReloadModel_ReturnsOk()
        {
            var response = await Client.PostAsync("/api/ollama/reload/llama3", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
