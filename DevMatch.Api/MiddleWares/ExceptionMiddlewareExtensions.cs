namespace DevMatch.Api.MiddleWares
{

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder
            UseGlobalExceptionHandling(
                this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
