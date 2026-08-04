using DevMatch.Application.Abstraction.Persistence;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Onboarding.ListSkills;

public sealed class ListOnboardingSkillsHandler
{
    private readonly IDevMatchDbContext _dbContext;

    public ListOnboardingSkillsHandler(IDevMatchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ListOnboardingSkillsResponse>> Handle(
        CancellationToken cancellationToken)
    {
        OnboardingSkillOption[] skills = await _dbContext.Skills
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new OnboardingSkillOption(
                x.Id,
                x.Name,
                x.Description))
            .ToArrayAsync(cancellationToken);

        return Result<ListOnboardingSkillsResponse>.Success(
            new ListOnboardingSkillsResponse(skills));
    }
}
