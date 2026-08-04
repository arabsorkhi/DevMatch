using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.Enums;
using DevMatch.Domain.ValueObjects;
using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Developer
{

    //ساختن User و Developer جداگانه فقط زمانی لازم می‌شود که بعداً این موارد را داشته باشی:

    //Admin غیر Developer
    //    Company Account
    //    Recruiter Account
    //    چند Authentication Provider
    //یک User با چند Profile متفاوت

    //برای MVP، همان Developer.GitHubUserId کافی است.




    //Domain نباید به EF وابسته شود.  no DataAnnotation
    // public sealed class Developer : AggregateRoot<Guid>
    //Developer
    // 
    // 1
    // 
    // ↓
    // 
    // ∞
    // 
    // Repository



    //Developer
    //     ↓
    // Developer Profile
    //     ↓
    // Skills
    //     ↓
    // Experience
    //     ↓
    // Interests
    //     ↓
    // Matching Engine
    //     ↓
    // Recommended Issues

    public sealed class Developer : AggregateRoot<Guid>
    {
        private readonly List<DeveloperSkill> _skills = [];
 
        private const int GitHubUsernameMaxLength = 100;
        private const int DisplayNameMaxLength = 200;
        private const int EmailMaxLength = 320;
        private const int AvatarUrlMaxLength = 2_000;
        private const int BioMaxLength = 2_000;
        private const int LocationMaxLength = 200;
        private const int CompanyMaxLength = 200;
        private const int BlogUrlMaxLength = 2_000;  

        public Guid Id { get; }
        public long GitHubUserId { get; private set; }  

        public string UserName { get; private set; } = null!;

        public string? GitHubUsername { get; private set; }
        public string NormalizedGitHubUsername { get; private set; } = string.Empty;
        public string? DisplayName { get; private set; }

        public string? Email { get; private set; }

        public string? AvatarUrl { get; private set; }

        public string? Bio { get; private set; }

        public string? Location { get; private set; }
        public string? Company { get; private set; }
        public string? BlogUrl { get; private set; }
        public string[] PreferredLanguages { get; private set; } = [];
        public string[] PreferredTopics { get; private set; } = [];
        public string[] ExcludedLabels { get; private set; } = [];
        public int? DailyAvailableMinutes { get; private set; }
        public bool AvoidDocumentation { get; private set; }
        public bool PreferBackend { get; private set; }
        public DateTimeOffset? OnboardingCompletedAtUtc { get; private set; }

        public bool IsDeleted { get; private set; }
 
        public DateTimeOffset? DeletedAtUtc { get; private set; }
        public bool IsAvailableForRecommendations { get; private set; } = true;
        public DateTimeOffset? GitHubProfileSyncedAtUtc { get; private set; }
        public DateTimeOffset? RepositoriesSyncedAtUtc { get; private set; }
        public DateTimeOffset? IssuesSyncedAtUtc { get; private set; }
        public DateTimeOffset CreatedAtUtc { get; private set; }
        public DateTimeOffset UpdatedAtUtc { get; private set; }

        private readonly List<GitRepository.GitRepository> _repositories =
            new(); //no need to : developer.AddRepository(...)

        //ساخت Repository در Handler انجام می‌شود.
        public IReadOnlyCollection<GitRepository.GitRepository> Repositories
            => _repositories.AsReadOnly();

        public IReadOnlyCollection<DeveloperSkill> Skills => _skills.AsReadOnly();

        public bool IsOnboardingCompleted => OnboardingCompletedAtUtc.HasValue;


        private Developer()
        {
        }
        private Developer(
            Guid id,
            long gitHubUserId,
            string gitHubUsername,
            string? displayName,
            string? email,
            string? avatarUrl,
            string? bio,
            string? location,
            DateTimeOffset utcNow)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Developer id cannot be empty.",
                    nameof(id));
            }

            if (gitHubUserId == long.MinValue)
            {
                throw new ArgumentException(
                    "GitHub user id cannot be empty.",
                    nameof(gitHubUserId));
            }
            Id = id;
            GitHubUserId = gitHubUserId;
            GitHubUsername = NormalizeRequired(
                gitHubUsername,
                nameof(gitHubUsername),
                GitHubUsernameMaxLength);

            NormalizedGitHubUsername =
                GitHubUsername.ToUpperInvariant();

            DisplayName = NormalizeOptional(
                displayName,
                nameof(displayName),
                DisplayNameMaxLength);

            Email = NormalizeOptional(
                email,
                nameof(email),
                EmailMaxLength);

            AvatarUrl = NormalizeOptional(
                avatarUrl,
                nameof(avatarUrl),
                AvatarUrlMaxLength);

            Bio = NormalizeOptional(
                bio,
                nameof(bio),
                BioMaxLength);

            Location = NormalizeOptional(
                location,
                nameof(location),
                LocationMaxLength);

            CreatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }



        public static Developer Create(
            long gitHubUserId,
            string gitHubUsername,
            string? displayName,
            string? email,
            string? avatarUrl,
            string? bio,
            string? location)
            => Create(
                gitHubUserId,
                gitHubUsername,
                displayName,
                email,
                avatarUrl,
                bio,
                location,
                DateTimeOffset.UtcNow);

        public static Developer Create(
            long gitHubUserId,
            string gitHubUsername,
            string? displayName,
            string? email,
            string? avatarUrl,
            string? bio,
            string? location,
            DateTimeOffset utcNow)
            => new(
                Guid.NewGuid(),
                gitHubUserId,
                gitHubUsername,
                displayName,
                email,
                avatarUrl,
                bio,
                location,
                utcNow);

        //  CreatedAtUtc = DateTime.UtcNow //Entity نباید بداند EF چه زمانی Save می‌کند.


        public void SynchronizeGitHubProfile(
               long gitHubUserId,
               string gitHubUsername,
               string? displayName,
               string? email,
               string? avatarUrl,
               string? bio,
               string? location,
               string? company,
               string? blogUrl,
               DateTimeOffset utcNow)
        {
            EnsureNotDeleted();

            GitHubUserId = gitHubUserId;
            GitHubUsername = NormalizeRequired(gitHubUsername);
            NormalizedGitHubUsername = GitHubUsername.ToUpperInvariant();
            DisplayName = NormalizeOptional(displayName, 200);
            Email = NormalizeOptional(email, 320);
            AvatarUrl = NormalizeOptional(avatarUrl, 2_000);
            Bio = NormalizeOptional(bio, 2_000);
            Location = NormalizeOptional(location, 200);
            Company = NormalizeOptional(company, 200);
            BlogUrl = NormalizeOptional(blogUrl, 2_000);
            GitHubProfileSyncedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
        public void SetOnboardingPreferences(
            IEnumerable<string> preferredLanguages,
            IEnumerable<string> preferredTopics,
            int? dailyAvailableMinutes,
            IEnumerable<string> excludedLabels,
            bool avoidDocumentation,
            bool preferBackend,
            DateTimeOffset utcNow)
        {
            EnsureNotDeleted();

            if (dailyAvailableMinutes is < 15 or > 1_440)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dailyAvailableMinutes),
                    "Daily available minutes must be between 15 and 1440.");
            }

            PreferredLanguages = NormalizeStringCollection(preferredLanguages, 50, 20);
            PreferredTopics = NormalizeStringCollection(preferredTopics, 100, 30);
            ExcludedLabels = NormalizeStringCollection(excludedLabels, 100, 30);
            DailyAvailableMinutes = dailyAvailableMinutes;
            AvoidDocumentation = avoidDocumentation;
            PreferBackend = preferBackend;
            OnboardingCompletedAtUtc = utcNow.ToUniversalTime();
            UpdatedAtUtc = utcNow.ToUniversalTime();
        }
        public void AddOrUpdateSkill(
            Guid skillId,
            SkillLevel level,
            ConfidenceScore confidence,
            bool isVerified,
            DeveloperSkillSource source,
            DateTimeOffset utcNow)
        {
            EnsureNotDeleted();

            if (skillId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Skill id cannot be empty.",
                    nameof(skillId));
            }
            DeveloperSkill? current = _skills.FirstOrDefault(x => x.SkillId == skillId);

            if (current is null)
            {
                _skills.Add(DeveloperSkill.Create(
                    Id,
                    skillId,
                    level,
                    confidence,
                    isVerified,
                    source,
                    utcNow));
                UpdatedAtUtc = utcNow;
                return;
            }

            current.Update(level, confidence, isVerified, source, utcNow);
            UpdatedAtUtc = utcNow;
        }

        public void RemoveSkill(Guid skillId, DateTimeOffset utcNow)
        {
            EnsureNotDeleted();

            DeveloperSkill? skill = _skills.FirstOrDefault(x => x.SkillId == skillId);
            if (skill is null)
            {
                return;
            }

            _skills.Remove(skill);
            UpdatedAtUtc = utcNow;
        }

        public void MarkRepositoriesSynced(DateTimeOffset utcNow)
        {
            RepositoriesSyncedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }

        public void MarkIssuesSynced(DateTimeOffset utcNow)
        {
            IssuesSyncedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
        public void SetRecommendationAvailability(
            bool isAvailable,
            DateTimeOffset utcNow)
        {
            EnsureNotDeleted();

            if (IsAvailableForRecommendations == isAvailable)
            {
                return;
            }

            IsAvailableForRecommendations = isAvailable;
            UpdatedAtUtc = utcNow.ToUniversalTime();
        }
        private void EnsureNotDeleted()
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException(
                    "A deleted developer cannot be modified.");
            }
        }

        //Entity نباید خودش تصمیم بگیرد «الان» چه زمانی است.این موضوع تست‌ها را deterministic نگه می‌دارد.
        public void SoftDelete(DateTimeOffset utcNow)
        {
            if (IsDeleted)
            {
                return;
            }

            IsDeleted = true;
            DeletedAtUtc = utcNow;
            IsAvailableForRecommendations = false;
            UpdatedAtUtc = utcNow;
        }

        private static string NormalizeRequired(string value)
            => value.Trim();

        private static string NormalizeRequired(
            string? value,
            string parameterName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"{parameterName} is required.",
                    parameterName);
            }

            string normalizedValue = value.Trim();

            if (normalizedValue.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{parameterName} cannot exceed {maxLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }

        private static string? NormalizeOptional(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value.Trim();
            return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
        }
        private static string? NormalizeOptional(
            string? value,
            string parameterName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalizedValue = value.Trim();

            if (normalizedValue.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{parameterName} cannot exceed {maxLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }
        private static string[] NormalizeStringCollection(
            IEnumerable<string> values,
            int itemMaxLength,
            int maxItems)
        {
            ArgumentNullException.ThrowIfNull(values);

            string[] result = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Select(value => value.Length <= itemMaxLength ? value : value[..itemMaxLength])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxItems)
                .ToArray();

            return result;
        }
    }


}
 
