using DevMatch.Application.Abstraction.Github;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevMatch.Api.Controllers
{

    [ApiController]
    [Route("api/admin/controlled-catalog")]
    [Authorize(Roles = "Admin")]
    public sealed class ControlledCatalogAdminController : ControllerBase
    {
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromServices] IRepositoryCatalogSyncOrchestrator orchestrator,
            CancellationToken cancellationToken)
        {
            return Ok(await orchestrator.GetSummaryAsync(cancellationToken));
        }

        [HttpGet("repositories")]
        public async Task<IActionResult> GetRepositories(
            [FromServices] IRepositoryCatalogAdminService adminService,
            CancellationToken cancellationToken)
        {
            var repositories = await adminService.ListAsync(cancellationToken);
            return Ok(repositories.Select(x => new
            {
                x.Id,
                x.FullName,
                x.PrimaryLanguage,
                x.IsEnabled,
                SelectionStatus = x.SelectionStatus.ToString(),
                x.SelectionReason,
                x.QualityScore,
                x.HasGoodFirstIssue,
                x.HasHelpWanted,
                x.HasReadme,
                x.HasContributionGuide,
                x.MaintainerResponseRate,
                x.MedianMaintainerResponseMinutes,
                x.GitHubPushedAt,
                x.LastSuccessfulSyncAt,
                SyncStatus = x.SyncState?.Status.ToString(),
                x.SyncState?.LastError,
                Topics = x.Topics.Select(topic => topic.Name).OrderBy(name => name)
            }));
        }

        [HttpPost("repositories")]
        public async Task<IActionResult> AddRepositories(
            [FromBody] AddRepositoryCandidatesRequest request,
            [FromServices] IRepositoryCatalogAdminService adminService,
            CancellationToken cancellationToken)
        {
            if (request.FullNames is null || request.FullNames.Count == 0)
            {
                return BadRequest(new { error = "At least one repository in owner/name format is required." });
            }

            var repositories = await adminService.AddCandidatesAsync(
                request.FullNames,
                cancellationToken);

            return Ok(repositories.Select(x => new { x.Id, x.FullName, x.SelectionStatus }));
        }

        [HttpPatch("repositories/{id:guid}/enabled")]
        public async Task<IActionResult> SetRepositoryEnabled(
            Guid id,
            [FromBody] SetRepositoryEnabledRequest request,
            [FromServices] IRepositoryCatalogAdminService adminService,
            CancellationToken cancellationToken)
        {
            await adminService.SetEnabledAsync(id, request.Enabled, cancellationToken);
            return NoContent();
        }

        [HttpPost("repositories/{id:guid}/sync")]
        public async Task<IActionResult> SyncRepository(
            Guid id,
            [FromServices] IRepositoryCatalogSyncOrchestrator orchestrator,
            CancellationToken cancellationToken)
        {
            return Ok(await orchestrator.SyncRepositoryAsync(id, cancellationToken));
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncAll(
            [FromServices] IRepositoryCatalogSyncOrchestrator orchestrator,
            CancellationToken cancellationToken)
        {
            return Ok(await orchestrator.SyncAllAsync(cancellationToken));
        }
    }

    public sealed record AddRepositoryCandidatesRequest(IReadOnlyCollection<string> FullNames);
    public sealed record SetRepositoryEnabledRequest(bool Enabled);

}
