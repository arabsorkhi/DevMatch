using System.Reflection;

namespace DevMatch.Api.Infrastructure
{
    //هر Feature خودش ثبت می‌شود.
    public static class EndpointExtensions
    {
        public static IServiceCollection AddEndpoints(
            this IServiceCollection services)
        {
            var endpoints = Assembly
                .GetExecutingAssembly()
                .DefinedTypes
                .Where(t =>
                    t is { IsAbstract: false, IsInterface: false } &&
                    typeof(IEndpoint).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .Cast<IEndpoint>();

            foreach (var endpoint in endpoints)
                services.AddSingleton(endpoint);

            return services;
        }

        public static IApplicationBuilder MapEndpoints(
            this WebApplication app)
        {
            var endpoints =
                app.Services.GetServices<IEndpoint>();

            foreach (var endpoint in endpoints)
                endpoint.MapEndpoint(app);

            return app;
        }
    }
}
