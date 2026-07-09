using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DevMatch.Api.MiddleWares
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var problemDetailsService =
                    context.RequestServices
                        .GetRequiredService<IProblemDetailsService>();
                context.Response.StatusCode = 500;

                //context.Response.ContentType =
                //    "application/problem+json";

                //var problem = new ProblemDetails
                //{
                //    Title = "Server Error",
                //    Detail = ex.Message,
                //    Status = 500,
                //    Type = "https://httpstatuses.com/500"
                //};

                //await context.Response.WriteAsync(
                //    JsonSerializer.Serialize(problem));



                await problemDetailsService.WriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = new ProblemDetails
                        {
                            Title = "Server Error",
                            Detail = ex.Message,
                            Status = 500
                        }
                    });
            }
        }
    }
    }
