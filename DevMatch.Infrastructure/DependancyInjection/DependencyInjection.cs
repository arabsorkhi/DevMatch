using DevMatch.Application.Abstraction;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Services;
using DevMatch.Infrastructure.Abstraction.Persistence;
using DevMatch.Infrastructure.Authentication.Github;
using DevMatch.Infrastructure.Authentication.Jwt;
using DevMatch.Infrastructure.Matching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Infrastructure.Security;

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
            services.Configure<GitHubOptions>(
                configuration.GetSection(GitHubOptions.SectionName));
            services.Configure<GitHubOAuthOptions>(
                configuration.GetSection(GitHubOAuthOptions.SectionName));
            services.Configure<JwtOptions>(
                configuration.GetSection(JwtOptions.SectionName));
            services.Configure<OAuthStateOptions>(
                configuration.GetSection(OAuthStateOptions.SectionName));
            services.Configure<GitHubTokenEncryptionOptions>(
                configuration.GetSection(GitHubTokenEncryptionOptions.SectionName));

            services.AddHttpClient<IGitHubClient, GitHubClient>((provider, client) =>
            {
                GitHubOptions options = provider.GetRequiredService<IOptions<GitHubOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            });

            services.AddHttpClient<IGitHubOAuthClient, GitHubOAuthClient>((provider, client) =>
            {
                GitHubOAuthOptions options = provider.GetRequiredService<IOptions<GitHubOAuthOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            });

            services.AddSingleton<IOAuthStateService, OAuthStateService>();
            services.AddSingleton<IGitHubTokenProtector, AesGcmGitHubTokenProtector>();
            services.AddSingleton<IAccessTokenProvider, AccessTokenProvider>();
            services.AddScoped<IGitHubTokenProvider, GitHubTokenProvider>();
            services.AddScoped<IMatchingProfileReader, MatchingProfileReader>();

            return services;
        }
    }
}
