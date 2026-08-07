using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common.Error;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.Enums;
using DevMatch.Domain.ValueObjects;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Profile;

public sealed class ProfileHandler
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public ProfileHandler(
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

    public async Task<Result<ProfileResponse>> GetAsync(CancellationToken cancellationToken)
    {
        Guid developerId = _currentUser.DeveloperId;

        Developer? developer = await _dbContext.Developers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == developerId, cancellationToken);

        if (developer is null)
        {
            return Result<ProfileResponse>.Failure(DeveloperErrors.NotFound);
        }

        DeveloperPreference? preference = await _dbContext.DeveloperPreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.DeveloperId == developerId, cancellationToken);

        var skillEntities = await _dbContext.DeveloperSkills
            .AsNoTracking()
            .Where(x => x.DeveloperId == developerId)
            .Include(x => x.Skill)
            .OrderBy(x => x.Skill.Name)
            .ToArrayAsync(cancellationToken);

        ProfileSkillResponse[] skills = skillEntities
            .Select(x => new ProfileSkillResponse(
                x.SkillId,
                x.Skill.Name,
                x.Level,
                x.Confidence.Value,
                x.IsVerified))
            .ToArray();

        return Result<ProfileResponse>.Success(Map(developer, preference, skills));
    }

    public async Task<Result<ProfileResponse>> UpdateAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        Guid developerId = _currentUser.DeveloperId;
        DateTimeOffset utcNow = _timeProvider.GetUtcNow();

        Developer? developer = await _dbContext.Developers
            .Include(x => x.Skills)
            .SingleOrDefaultAsync(x => x.Id == developerId, cancellationToken);

        if (developer is null)
        {
            return Result<ProfileResponse>.Failure(DeveloperErrors.NotFound);
        }

        DeveloperPreference? preference = await _dbContext.DeveloperPreferences
            .SingleOrDefaultAsync(x => x.DeveloperId == developerId, cancellationToken);

        if (preference is null)
        {
            preference = DeveloperPreference.Create(developerId, utcNow);
            await _dbContext.DeveloperPreferences.AddAsync(preference, cancellationToken);
        }

        preference.Update(
            command.Level,
            command.PreferredLanguages,
            command.PreferredTopics,
            command.ExcludedLabels,
            command.DailyAvailableMinutes,
            command.AvoidDocumentation,
            command.PreferBackend,
            utcNow);

        string[] requestedNames = command.Skills
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string[] normalizedNames = requestedNames
            .Select(x => x.ToUpperInvariant())
            .ToArray();

        Dictionary<string, Skill> skillByName = await _dbContext.Skills
            .Where(x => normalizedNames.Contains(x.NormalizedName))
            .ToDictionaryAsync(x => x.NormalizedName, cancellationToken);

        foreach (string name in requestedNames)
        {
            string normalized = name.ToUpperInvariant();
            if (skillByName.ContainsKey(normalized))
            {
                continue;
            }

            Skill skill = Skill.Create(name, description: null);
            await _dbContext.Skills.AddAsync(skill, cancellationToken);
            skillByName[normalized] = skill;
        }

        HashSet<Guid> requestedSkillIds = [];
        foreach (ProfileSkillInput input in command.Skills)
        {
            Skill skill = skillByName[input.Name.Trim().ToUpperInvariant()];
            requestedSkillIds.Add(skill.Id);
            developer.AddOrUpdateSkill(
                skill.Id,
                input.Level,
                ConfidenceScore.Full,
                isVerified: true,
                DeveloperSkillSource.Manual,
                utcNow);
        }

        foreach (Guid skillId in developer.Skills
                     .Where(x => x.Source == DeveloperSkillSource.Manual && !requestedSkillIds.Contains(x.SkillId))
                     .Select(x => x.SkillId)
                     .ToArray())
        {
            developer.RemoveSkill(skillId, utcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetAsync(cancellationToken);
    }

    private static ProfileResponse Map(
        Developer developer,
        DeveloperPreference? preference,
        IReadOnlyCollection<ProfileSkillResponse> skills) =>
        new(
            developer.Id,
            developer.UserName,
            developer.DisplayName,
            developer.AvatarUrl,
            preference?.SelfReportedLevel ?? SkillLevel.Unknown,
            preference?.PreferredLanguages ?? [],
            preference?.PreferredTopics ?? [],
            preference?.ExcludedLabels ?? [],
            preference?.DailyAvailableMinutes,
            preference?.AvoidDocumentation ?? false,
            preference?.PreferBackend ?? false,
            skills);
}
