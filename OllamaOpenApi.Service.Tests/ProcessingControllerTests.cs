using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;
using Xunit;

namespace OllamaOpenApi.Service.Tests
{
    public class ProcessingControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task SubmitQuery_WithValidPrompt_ReturnsCreated()
        {
            var request = new SubmitQueryRequest("Hello world", null);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/Processing/submit", content);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task SubmitQuery_WithEmptyPrompt_ReturnsBadRequest()
        {
            var request = new SubmitQueryRequest("", null);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/Processing/submit", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ProcessSessions_WithValidIds_ReturnsAccepted()
        {
            var request = new QueueSessionsRequest(new List<Guid> { Guid.NewGuid() });
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/Processing/process", content);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task ProcessSessions_WithEmptyIds_ReturnsBadRequest()
        {
            var request = new QueueSessionsRequest(new List<Guid>());
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/Processing/process", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Pause_ReturnsNoContent()
        {
            var response = await Client.PostAsync("/api/Processing/pause", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Resume_ReturnsNoContent()
        {
            var response = await Client.PostAsync("/api/Processing/resume", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Interrupt_ReturnsNoContent()
        {
            var response = await Client.PostAsync("/api/Processing/interrupt", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetActiveSession_WhenNoneActive_ReturnsNotFound()
        {
            var response = await Client.GetAsync("/api/Processing/activeSession");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetStatus_ReturnsOk()
        {
            var response = await Client.GetAsync("/api/Processing/status");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
