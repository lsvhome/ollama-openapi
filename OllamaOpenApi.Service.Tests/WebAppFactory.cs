using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaOpenApi.Service.Services;

namespace OllamaOpenApi.Service.Tests
{
    public class WebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IOllamaAgentService, MockOllamaAgentService>();
            });
        }

        public OllamaAgentService Agent => Services.GetRequiredService<OllamaAgentService>();
    }

    public abstract class WebApiTestBase : IDisposable
    {
        protected WebAppFactory Factory { get; } = new();
        protected HttpClient Client { get; }

        protected WebApiTestBase()
        {
            Client = Factory.CreateClient();
        }

        public void Dispose() => Factory.Dispose();
    }
}
