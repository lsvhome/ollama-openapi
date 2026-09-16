using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using OllamaOpenApi.Dto;
using OllamaOpenApi.Service.Controllers;

namespace OllamaOpenApi.Service.Tests
{
    public class SessionsControllerTests : WebApiTestBase
    {
        private async Task<Guid> CreateSessionAsync()
        {
            var id = Guid.NewGuid();
            var response = await Client.PostAsJsonAsync("/api/Processing/submit", new SubmitQueryRequest("hello", id));
            response.EnsureSuccessStatusCode();
            return id;
        }

        [Fact]
        public async Task GetSessions_ReturnsOk_AndEmptyList()
        {
            await Client.DeleteAsync("/api/sessions/clear");

            var response = await Client.GetAsync("/api/sessions/list");
            response.EnsureSuccessStatusCode();

            var sessions = await response.Content.ReadFromJsonAsync<System.Collections.Generic.IReadOnlyList<SessionDto>>();
            Assert.NotNull(sessions);
            Assert.Empty(sessions);
        }

        [Fact]
        public async Task CreateSession_ValidRequest_ReturnsOkAndSessionId()
        {
            var request = new CreateSessionRequest("llama3", 1, 1, "You are helpful.", null, null, null);
            var response = await Client.PostAsJsonAsync("/api/sessions/create", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreateSessionResponse>();
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.SessionId);
        }

        [Fact]
        public async Task CreateSession_WithSystemPromptText_ReturnsOkAndSessionId()
        {
            var request = new CreateSessionRequest("llama3", null, 1, "You are helpful.", null, "Custom System Prompt", null);
            var response = await Client.PostAsJsonAsync("/api/sessions/create", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreateSessionResponse>();
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.SessionId);
        }

        [Fact]
        public async Task ExportSession_UnknownId_ReturnsNotFound()
        {
            var response = await Client.GetAsync($"/api/sessions/{Guid.NewGuid()}/export");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ExportSession_KnownId_ReturnsJsonFile()
        {
            var sessionId = await CreateSessionAsync();
            var response = await Client.GetAsync($"/api/sessions/{sessionId}/export");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(sessionId.ToString(), body);
        }

        [Fact]
        public async Task ImportSession_EmptyPayload_ReturnsBadRequest()
        {
            var form = new MultipartFormDataContent();
            var response = await Client.PostAsync($"/api/sessions/{Guid.NewGuid()}/import", form);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ImportSession_ValidPayload_ReturnsNoContent_AndSessionIsListed()
        {
            var id = Guid.NewGuid();
            var session = new Session { Id = id, SystemPrompt = "test prompt" };
            var payload = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(session));

            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(payload), "file", "session.json");
            var response = await Client.PostAsync($"/api/sessions/{id}/import", form);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var list = await Client.GetFromJsonAsync<System.Collections.Generic.IReadOnlyList<SessionDto>>("/api/sessions/list");
            Assert.NotNull(list);
            Assert.Contains(list, s => s.Id == id);
        }

        [Fact]
        public async Task ExportAllSessions_ReturnsJsonWithEnvelope()
        {
            await CreateSessionAsync();
            var response = await Client.GetAsync("/api/sessions/export-all");
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"Sessions\"", body);
            Assert.Contains("\"Version\"", body);
        }

        [Fact]
        public async Task ImportAllSessions_EmptyPayload_ReturnsBadRequest()
        {
            var form = new MultipartFormDataContent();
            var response = await Client.PostAsync("/api/sessions/import-all", form);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ImportAllSessions_ValidPayload_ReturnsNoContent_AndSessionsAreListed()
        {
            var s1 = Guid.NewGuid();
            var s2 = Guid.NewGuid();
            var envelope = new
            {
                version = 1,
                exportedAt = DateTime.UtcNow,
                sessions = new[]
                {
                    new Session { Id = s1, SystemPrompt = "p1" },
                    new Session { Id = s2, SystemPrompt = "p2" }
                }
            };
            var payload = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(envelope));

            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(payload), "file", "sessions-export.json");
            var response = await Client.PostAsync("/api/sessions/import-all", form);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var list = await Client.GetFromJsonAsync<System.Collections.Generic.IReadOnlyList<SessionDto>>("/api/sessions/list");
            Assert.NotNull(list);
            Assert.Contains(list, s => s.Id == s1);
            Assert.Contains(list, s => s.Id == s2);
        }

        [Fact]
        public async Task DeleteSession_KnownId_RemovesSession()
        {
            var sessionId = await CreateSessionAsync();
            var del = await Client.DeleteAsync($"/api/sessions/{sessionId}");
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

            var list = await Client.GetFromJsonAsync<System.Collections.Generic.IReadOnlyList<SessionDto>>("/api/sessions/list");
            Assert.NotNull(list);
            Assert.DoesNotContain(list, s => s.Id == sessionId);
        }

        [Fact]
        public async Task ClearAllSessions_RemovesAllSessions()
        {
            await CreateSessionAsync();
            await CreateSessionAsync();

            var del = await Client.DeleteAsync("/api/sessions/clear");
            Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

            var list = await Client.GetFromJsonAsync<System.Collections.Generic.IReadOnlyList<SessionDto>>("/api/sessions/list");
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Fact]
        public async Task CreateSession_WithWorkingFolder_ReturnsOkAndSessionId()
        {
            var workingFolder = "/tmp/test-folder";
            var request = new CreateSessionRequest("llama3", 1, 1, "You are helpful.", null, null, workingFolder);
            var response = await Client.PostAsJsonAsync("/api/sessions/create", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreateSessionResponse>();
            Assert.NotNull(result);

            var session = await Client.GetFromJsonAsync<SessionDto>($"/api/sessions/{result.SessionId}");
            Assert.Equal(workingFolder, session.WorkingFolder);
        }

        [Fact]
        public async Task UpdateSessionWorkingFolder_ValidRequest_ReturnsNoContent()
        {
            var sessionId = await CreateSessionAsync();
            var newFolder = "/tmp/new-folder";
            var request = new UpdateSessionWorkingFolderRequest(newFolder);
            var response = await Client.PutAsJsonAsync($"/api/sessions/{sessionId}/working-folder", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var session = await Client.GetFromJsonAsync<SessionDto>($"/api/sessions/{sessionId}");
            Assert.Equal(newFolder, session.WorkingFolder);
        }

        [Fact]
        public async Task GetSession_KnownId_ReturnsOk()
        {
            var sessionId = await CreateSessionAsync();
            var response = await Client.GetAsync($"/api/sessions/{sessionId}");
            response.EnsureSuccessStatusCode();

            var session = await response.Content.ReadFromJsonAsync<SessionDto>();
            Assert.NotNull(session);
            Assert.Equal(sessionId, session.Id);
        }

        [Fact]
        public async Task GetSession_UnknownId_ReturnsNotFound()
        {
            var response = await Client.GetAsync($"/api/sessions/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSessionTitle_ValidRequest_ReturnsNoContent()
        {
            var sessionId = await CreateSessionAsync();
            var newTitle = "Updated Title";
            var request = new UpdateSessionTitleRequest(newTitle);
            var response = await Client.PutAsJsonAsync($"/api/sessions/{sessionId}/title", request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var session = await Client.GetFromJsonAsync<SessionDto>($"/api/sessions/{sessionId}");
            Assert.Equal(newTitle, session.Title);
        }

        [Fact]
        public async Task UpdateSessionTitle_EmptyTitle_ReturnsBadRequest()
        {
            var sessionId = await CreateSessionAsync();
            var request = new UpdateSessionTitleRequest("");
            var response = await Client.PutAsJsonAsync($"/api/sessions/{sessionId}/title", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SaveSessions_ReturnsOk()
        {
            var response = await Client.PostAsync("/api/sessions/save", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }

    public sealed record CreateSessionResponse(Guid SessionId);
}
