using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DevMatch.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {

            ////هیچ Handler Register نمی‌شود. همه خودکار هستند.
            //services.Scan(scan =>
            //{
            //    scan.FromAssemblyOf<DependencyInjection>()
            //        .AddClasses(c => c.Where(t =>
            //            t.Name.EndsWith("Handler")))
            //        .AsSelf()
            //        .WithScopedLifetime();
            //});

            //services.AddValidatorsFromAssembly(
            //    Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
