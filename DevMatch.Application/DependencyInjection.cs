using DevMatch.Application.Abstraction;
using DevMatch.Application.Features.Authentication.Github.BeginLogin;
using DevMatch.Domain.Entities.Matching;
using DevMatch.Domain.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DevMatch.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {

            services.AddSingleton(TimeProvider.System);
          //  services.AddSingleton(MatchingWeights.Default);
            services.AddSingleton(new MatchingWeights
            {
                Skill = 0.35m,
                Repository = 0.10m,
                Contribution = 0.10m,
                Activity = 0.10m,
                Preference = 0.15m,
                History = 0.10m,
                Level = 0.10m
            });

            services.AddScoped<IMatchingEngine, BasicMatchingEngine>();

            services.AddScoped<IMatchingService, MatchingService>();
         //   services.AddHttpClient<IGitHubOAuthClient, GitHubOAuthClient>();
            services.AddScoped<BeginGitHubLoginHandler>();
            var assembly = typeof(DependencyInjection).Assembly;

            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith(
                        "Handler",
                        StringComparison.Ordinal)))
                .AsSelf()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblies(assembly)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(IValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
 }
