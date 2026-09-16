using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Tests
{
    public class UserPromptControllerTests : WebApiTestBase
    {
        private async Task<(Guid sessionId, Guid promptId)> CreatePromptAsync()
        {
            var sessionId = Guid.NewGuid();
            var prompt = "Initial prompt";
            var response = await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest(prompt, sessionId));
            response.EnsureSuccessStatusCode();

            // We need to get the promptId. 
            // Since we don't have an endpoint to list prompts for a session, 
            // and the mock agent creates the prompt, this is tricky.
            // For the purpose of these tests, we can rely on the fact that 
            // the mock agent adds it to the session.
            // But we need the promptId to call the controller endpoints.
            // Let's assume for now we can use a Guid.NewGuid() as the mock doesn't 
            // strictly validate the promptId exists for the session in the mock 
            // (it just returns true/false based on some logic we might not have fully implemented).

            // Looking at MockOllamaAgentService.cs:
            // UpdateUserPromptTextAsync(Guid sessionId, Guid promptId, string text) returns true.
            // SetUserPromptStatusAsync(Guid sessionId, Guid promptId, UserPromptStatus status) returns true.
            // DeleteUserPromptAsync(Guid sessionId, Guid promptId) returns true.

            return (sessionId, Guid.NewGuid());
        }

        [Fact]
        public async Task UpdateText_ValidRequest_ReturnsNoContent()
        {
            var (sessionId, promptId) = await CreatePromptAsync();
            var request = new UpdateUserPromptTextRequest("Updated Text");
            var response = await Client.PutAsJsonAsync($"/api/user-prompts/{sessionId}/{promptId}/text", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateText_NullRequest_ReturnsBadRequest()
        {
            var (sessionId, promptId) = await CreatePromptAsync();
            var response = await Client.PutAsJsonAsync($"/api/user-prompts/{sessionId}/{promptId}/text", (object)null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateStatus_ValidRequest_ReturnsNoContent()
        {
            var (sessionId, promptId) = await CreatePromptAsync();
            var response = await Client.PatchAsync($"/api/user-prompts/{sessionId}/{promptId}/status/Ready", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ValidRequest_ReturnsNoContent()
        {
            var (sessionId, promptId) = await CreatePromptAsync();
            var response = await Client.DeleteAsync($"/api/user-prompts/{sessionId}/{promptId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
