using System.Reflection;

namespace DevMatch.Api.Infrastructure
{
    //هر Feature خودش ثبت می‌شود.
    public static class EndpointExtensions
    {
        public static IServiceCollection AddEndpoints(
            this IServiceCollection services,
            Assembly assembly)
        {
            var endpointTypes =
                assembly
                    .DefinedTypes
                    .Where(type =>
                        !type.IsAbstract &&
                        !type.IsInterface &&
                        typeof(IEndpoint).IsAssignableFrom(type))
                    .ToArray();

            foreach (var endpointType in endpointTypes)
            {
              //Transient : Endpointها هیچ State نگه نمی‌دارند.هر Request یک Instance جدید می‌گیرد.
                services.AddTransient(
                    typeof(IEndpoint),
                    endpointType);
            }

            return services;
        }

        public static WebApplication MapEndpoints(
            this WebApplication app)
        {
            var endpoints =
                app.Services
                    .GetRequiredService<IEnumerable<IEndpoint>>();

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoint(app);
            }

            return app;
        }
    }
}
 
