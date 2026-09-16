using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OllamaOpenApi.Service.Tests
{
    public class FoldersControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task ListFolders_WithCurrentDirectory_ReturnsOk()
        {
            var response = await Client.GetAsync("/api/folders/list");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ListFolders_WithNonExistentDirectory_ReturnsNotFound()
        {
            var response = await Client.GetAsync("/api/folders/list?path=/non/existent/directory/that/should/not/exist");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
