using System.Diagnostics;

namespace DevMatch.Api.MiddleWares
{
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            long started = Stopwatch.GetTimestamp();
            try
            {
                await _next(context);
            }
            finally
            {
                TimeSpan elapsed = Stopwatch.GetElapsedTime(started);
                string? developerId = context.User.FindFirst("sub")?.Value;

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:F2} ms. TraceId: {TraceId}, DeveloperId: {DeveloperId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds,
                    context.TraceIdentifier,
                    developerId);
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestLoggingMiddleware>();
    }

}