using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OllamaOpenApi.Service.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;

        public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
            {
                await _next(context);
                return;
            }

            // Log Request
            context.Request.EnableBuffering();
            var requestBody = await ReadStream(context.Request.Body);
            _logger.LogDebug("Incoming Request: {Method} {Path}\nBody: {Body}",
                context.Request.Method, context.Request.Path, requestBody);

            context.Request.Body.Position = 0;

            // Capture Response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);

                // Log Response
                var responseBody = await ReadStream(responseBodyStream);
                _logger.LogDebug("Outgoing Response: {StatusCode}\nBody: {Body}",
                    context.Response.StatusCode, responseBody);

                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<string> ReadStream(Stream stream)
        {
            if (stream == null) return "[Null Stream]";

            var originalPosition = stream.Position;
            using var reader = new StreamReader(
                stream,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            stream.Position = originalPosition;

            return string.IsNullOrEmpty(body) ? "[Empty]" : body;
        }
    }
}
