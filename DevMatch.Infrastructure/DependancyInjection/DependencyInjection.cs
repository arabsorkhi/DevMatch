using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Services;
using DevMatch.Infrastructure.Abstraction.Persistence;
using DevMatch.Infrastructure.Matching;
using Microsoft.Extensions.Options;
using DevMatch.Infrastructure.Authentication.Github;
using DevMatch.Application.Abstraction.Authentication.Github;

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

            services.Configure<GitHubOptions>(configuration.GetSection(GitHubOptions.SectionName));
            services.AddHttpClient<IGitHubClient, GitHubClient>((provider, client) =>
            {
                GitHubOptions options = provider.GetRequiredService<IOptions<GitHubOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            });
            services.AddScoped<
                IMatchingProfileReader, MatchingProfileReader>();
            //services
            //    .Configure<GitHubOptions>(configuration.GetSection(
            //            GitHubOptions.SectionName));
            return services;
        }
    }
}
