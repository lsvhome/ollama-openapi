using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using OllamaOpenApi.Dto;
using System.Collections.Generic;

namespace OllamaOpenApi.Service.Tests
{
    public class SystemPromptControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var response = await Client.GetAsync("/api/SystemPrompt/list");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var request = new SystemPromptRequest("Test System Prompt");
            var response = await Client.PutAsJsonAsync("/api/SystemPrompt", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNoContent()
        {
            var request = new SystemPromptRequest("Updated Prompt");
            var response = await Client.PutAsJsonAsync("/api/SystemPrompt/1", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent()
        {
            var response = await Client.DeleteAsync("/api/SystemPrompt/1");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetById_Exists_ReturnsOk()
        {
            // First create one to ensure it exists if the service uses in-memory storage that starts empty
            var request = new SystemPromptRequest("Existing Prompt");
            var createResponse = await Client.PutAsJsonAsync("/api/SystemPrompt", request);
            createResponse.EnsureSuccessStatusCode();

            var response = await Client.GetAsync("/api/SystemPrompt/1");
            // Depending on how IDs are generated, it might be 1 or something else. 
            // If it's not found, it returns NotFound.
            if (response.StatusCode != HttpStatusCode.OK)
            {
                // If not 1, try to get the created one from location header
                var location = createResponse.Headers.Location;
                var getResponse = await Client.GetAsync(location!.ToString());
                Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
