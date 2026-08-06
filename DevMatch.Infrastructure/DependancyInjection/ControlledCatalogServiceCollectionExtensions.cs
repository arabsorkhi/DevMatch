using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Common.Option;
using DevMatch.Infrastructure.Abstraction.Persistence;
using DevMatch.Infrastructure.ControlledCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Infrastructure.Services;

namespace DevMatch.Infrastructure.DependancyInjection
{
    public static class ControlledCatalogServiceCollectionExtensions
    {
        public static IServiceCollection AddControlledCatalog(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<DbContextOptionsBuilder> configureDatabase)
        {
            services
                .AddOptions<ControlledCatalogOptions>()
                .Bind(configuration.GetSection(ControlledCatalogOptions.SectionName))
                .Validate(
                    x => x.MinRepositories > 0 && x.MaxRepositories >= x.MinRepositories,
                    "Controlled repository bounds are invalid.")
                .Validate(
                    x => x.MinIssues > 0 && x.MaxIssues >= x.MinIssues,
                    "Controlled issue bounds are invalid.")
                .ValidateOnStart();

            services
                .AddOptions<GitHubCatalogOptions>()
                .Bind(configuration.GetSection(GitHubCatalogOptions.SectionName))
                .Validate(
                    x => Uri.TryCreate(x.ApiBaseUrl, UriKind.Absolute, out _),
                    "GitHubCatalog:ApiBaseUrl must be an absolute URL.")
                .ValidateOnStart();

            services.AddDbContext<DevMatchDbContext>(configureDatabase);

            services.AddHttpClient<IGitHubCatalogClient, GitHubCatalogClient>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<GitHubCatalogOptions>>().Value;
                client.BaseAddress = new Uri(options.ApiBaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

                if (!string.IsNullOrWhiteSpace(options.AccessToken))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", options.AccessToken);
                }

                client.Timeout = TimeSpan.FromSeconds(60);
            });

            services.AddScoped<IRepositoryQualityEvaluator, RepositoryQualityEvaluator>();
            services.AddScoped<IRepositoryCatalogAdminService, RepositoryCatalogAdminService>();
            services.AddScoped<IRepositoryCatalogSyncOrchestrator, RepositoryCatalogSyncOrchestrator>();
          //  services.AddHostedService<IssueCatalogSyncWorker>();

            return services;
        }
    }
}