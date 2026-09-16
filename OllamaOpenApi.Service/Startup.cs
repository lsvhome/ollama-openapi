using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaOpenApi.Service.Data;
using OllamaOpenApi.Service.Services;
using OllamaOpenApi.Service.Middleware;
using Scalar.AspNetCore;

namespace OllamaOpenApi.Service
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Add CORS service and configure policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    if (Environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                    else
                    {
                        // For production environment, specify the exact frontend URL
                        policy.WithOrigins("https://your-production-frontend.com", "http://localhost:5023", "http://localhost:11436", "https://localhost:7127")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            services.AddControllers();
            services.AddHttpClient();
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSingleton<IMcpConfigService, McpConfigService>();

            // Register the Agent Service as a Singleton and Hosted Service
            services.AddSingleton<OllamaAgentService>();
            services.AddHostedService(sp => sp.GetRequiredService<OllamaAgentService>());
            services.AddSingleton<IOllamaAgentService>(sp => sp.GetRequiredService<OllamaAgentService>());
        }

        public void Configure(WebApplication app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // 2. Enable CORS (MANDATORY between UseRouting and UseAuthorization)
            app.UseCors("AllowFrontend");

            app.UseAuthorization();
            app.UseMiddleware<OllamaOpenApi.Service.Middleware.ExceptionHandlingMiddleware>();
            app.UseMiddleware<HttpLoggingMiddleware>();
            app.MapControllers();
        }
    }
}
