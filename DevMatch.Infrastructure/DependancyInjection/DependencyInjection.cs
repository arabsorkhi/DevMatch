using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Infrastructure.Abstraction.Persistence;

namespace DevMatch.Infrastructure.DependancyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<DevMatchDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IDevMatchDbContext>(
                provider =>
                    provider.GetRequiredService<DevMatchDbContext>());
             

            return services;
        }
    }
}
