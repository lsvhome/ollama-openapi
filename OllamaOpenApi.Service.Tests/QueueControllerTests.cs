using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;

namespace OllamaOpenApi.Service.Tests
{
    public class QueueControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task SubmitQuery_EmptyPrompt_ReturnsBadRequest()
        {
            var response = await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest("", null));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SubmitQuery_WithoutSessionId_CreatesNewSession()
        {
            var response = await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest("hello", null));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.Location;
            Assert.NotNull(location);

            var path = location!.ToString();
            var sessionId = Guid.Parse(path.TrimEnd('/').Split('/').Last());
            var session = await Client.GetFromJsonAsync<SessionDto>($"/api/sessions/{sessionId}");
            Assert.NotNull(session);
            Assert.Equal(sessionId, session.Id);
            Assert.Single(session.UserPrompts);
        }

        [Fact]
        public async Task SubmitQuery_WithExistingSessionId_AppendsQuery()
        {
            var id = Guid.NewGuid();
            var first = await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest("first", id));
            first.EnsureSuccessStatusCode();

            var second = await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest("second", id));
            second.EnsureSuccessStatusCode();

            var session = await Client.GetFromJsonAsync<SessionDto>($"/api/sessions/{id}");
            Assert.NotNull(session);
            Assert.Equal(id, session.Id);
            Assert.Equal(2, session.UserPrompts.Count);
        }

        [Fact]
        public async Task QueueSessions_EmptyList_ReturnsBadRequest()
        {
            var response = await Client.PostAsJsonAsync("/api/Processing/process", new QueueSessionsRequest(Array.Empty<Guid>()));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task QueueSessions_ValidIds_ReturnsAccepted()
        {
            var id = Guid.NewGuid();
            await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest("hello", id));

            var response = await Client.PostAsJsonAsync("/api/Processing/process", new QueueSessionsRequest(new[] { id }));
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task PauseQueue_ReturnsNoContent()
        {
            var response = await Client.PostAsync("/api/Processing/pause", null);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ResumeQueue_ReturnsNoContent()
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
        public async Task GetActiveSession_NoActive_ReturnsNotFound()
        {
            var response = await Client.GetAsync("/api/Processing/activeSession");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetSession_UnknownId_ReturnsNotFound()
        {
            var response = await Client.GetAsync($"/api/sessions/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
