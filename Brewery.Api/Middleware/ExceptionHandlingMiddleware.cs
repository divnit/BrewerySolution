using System.Net;
using System.Text.Json;

namespace Brewery.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { code = "bad_request", message = ex.Message, traceId = context.TraceIdentifier }));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Upstream error");
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { code = "upstream_error", message = "Failed to fetch data from upstream", traceId = context.TraceIdentifier }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { code = "server_error", message = "An unexpected error occurred", traceId = context.TraceIdentifier }));
            }
        }
    }
}
