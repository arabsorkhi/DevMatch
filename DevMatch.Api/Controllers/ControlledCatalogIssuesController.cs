using DevMatch.Domain.Enums;
using DevMatch.Infrastructure.Abstraction.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Api.Controllers
{

    [ApiController]
    [Route("api/controlled-catalog/issues")]
    [Authorize]
    public sealed class ControlledCatalogIssuesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetIssues(
            [FromServices] DevMatchDbContext dbContext,
            [FromQuery] string? language,
            [FromQuery] bool? goodFirstIssue,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 100);

            var query = dbContext.IssueCandidates
                .AsNoTracking()
                .Where(x =>
                    x.IsInControlledSet &&
                    x.IsEligible &&
                    x.State == IssueCandidateState.Open);

            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(x => x.RepositorySource.PrimaryLanguage == language);
            }

            if (goodFirstIssue is not null)
            {
                query = query.Where(x => x.IsGoodFirstIssue == goodFirstIssue.Value);
            }

            var issues = await query
                .OrderByDescending(x => x.IsGoodFirstIssue)
                .ThenByDescending(x => x.CandidateScore)
                .ThenByDescending(x => x.GitHubUpdatedAt)
                .Take(take)
                .Select(x => new
                {
                    x.Id,
                    Repository = x.RepositorySource.FullName,
                    Language = x.RepositorySource.PrimaryLanguage,
                    x.Number,
                    x.Title,
                    x.HtmlUrl,
                    x.IsGoodFirstIssue,
                    x.IsHelpWanted,
                    x.EstimatedMinutes,
                    x.DifficultyScore,
                    x.CandidateScore,
                    x.CommentsCount,
                    x.AssigneeCount,
                    x.GitHubUpdatedAt
                })
                .ToArrayAsync(cancellationToken);

            return Ok(issues);
        }
    }
}
