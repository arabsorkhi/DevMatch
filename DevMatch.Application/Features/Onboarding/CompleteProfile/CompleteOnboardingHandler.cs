using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.Enums;
using DevMatch.Domain.ValueObjects;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Onboarding.CompleteProfile;

public sealed class CompleteOnboardingHandler
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public CompleteOnboardingHandler(
        IDevMatchDbContext dbContext,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CompleteOnboardingResponse>> Handle(
        CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        Developer? developer = await _dbContext.Developers
            .Include(x => x.Skills)
            .SingleOrDefaultAsync(x => x.Id == _currentUser.DeveloperId, cancellationToken);

        if (developer is null)
        {
            return Result<CompleteOnboardingResponse>.Failure(
                AuthenticationErrors.DeveloperNotFound);
        }

        Guid[] requestedSkillIds = command.Skills
            .Select(x => x.SkillId)
            .Distinct()
            .ToArray();

        Dictionary<Guid, Skill> skillsById = await _dbContext.Skills
            .Where(x => requestedSkillIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        Guid[] missingSkillIds = requestedSkillIds
            .Where(id => !skillsById.ContainsKey(id))
            .ToArray();

        if (missingSkillIds.Length > 0)
        {
            return Result<CompleteOnboardingResponse>.Failure(Error.Validation(
                "Onboarding.InvalidSkills",
                $"Unknown or inactive skill ids: {string.Join(", ", missingSkillIds)}"));
        }

        DateTimeOffset utcNow = _timeProvider.GetUtcNow();
        HashSet<Guid> requestedSet = requestedSkillIds.ToHashSet();

        Guid[] removedManualSkills = developer.Skills
            .Where(x => x.Source == DeveloperSkillSource.Manual && !requestedSet.Contains(x.SkillId))
            .Select(x => x.SkillId)
            .ToArray();

        foreach (Guid skillId in removedManualSkills)
            developer.RemoveSkill(skillId, utcNow);

        foreach (OnboardingSkillInput input in command.Skills)
        {
            developer.AddOrUpdateSkill(
                input.SkillId,
                input.Level,
                ConfidenceScore.Full,
                isVerified: true,
                DeveloperSkillSource.Manual,
                utcNow);
        }

        developer.SetOnboardingPreferences(
            command.PreferredLanguages,
            command.PreferredTopics,
            command.DailyAvailableMinutes,
            command.ExcludedLabels,
            command.AvoidDocumentation,
            command.PreferBackend,
            utcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        OnboardingSkillResponse[] skillResponses = command.Skills
            .Select(input => new OnboardingSkillResponse(
                input.SkillId,
                skillsById[input.SkillId].Name,
                input.Level))
            .ToArray();

        return Result<CompleteOnboardingResponse>.Success(new CompleteOnboardingResponse(
            developer.Id,
            developer.IsOnboardingCompleted,
            developer.OnboardingCompletedAtUtc!.Value,
            skillResponses,
            developer.PreferredLanguages,
            developer.PreferredTopics,
            developer.DailyAvailableMinutes,
            developer.ExcludedLabels,
            developer.AvoidDocumentation,
            developer.PreferBackend));
    }
}
