using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Common.Option;

namespace DevMatch.Worker
{

    public sealed class IssueCatalogSyncWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ControlledCatalogOptions _options;
        private readonly ILogger<IssueCatalogSyncWorker> _logger;

        public IssueCatalogSyncWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<ControlledCatalogOptions> options,
            ILogger<IssueCatalogSyncWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_options.InitialDelaySeconds > 0)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(_options.InitialDelaySeconds),
                    stoppingToken);
            }

            await RunOnceAsync(stoppingToken);

            using var timer = new PeriodicTimer(
                TimeSpan.FromMinutes(Math.Max(1, _options.SyncIntervalMinutes)));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunOnceAsync(stoppingToken);
            }
        }

        private async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider
                    .GetRequiredService<IRepositoryCatalogSyncOrchestrator>();

                var result = await orchestrator.SyncAllAsync(cancellationToken);
                _logger.LogInformation(
                    "Controlled catalog sync completed. Repositories: {Succeeded}/{Attempted}; controlled issues: {IssueCount}",
                    result.RepositoriesSucceeded,
                    result.RepositoriesAttempted,
                    result.IssuesInControlledSet);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Normal service shutdown.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Controlled catalog worker run failed.");
            }
        }
    }

}
