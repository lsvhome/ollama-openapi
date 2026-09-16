using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OllamaOpenApi.Service.Tests
{
    public class VersionControllerTests : WebApiTestBase
    {
        [Fact]
        public async Task GetVersion_ReturnsOk()
        {
            var response = await Client.GetAsync("/api/version");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
