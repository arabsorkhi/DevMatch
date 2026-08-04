using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.GitRepository
{
    //اما برای GitRepository و GitIssue پیشنهاد من این است که اصلاً Create و Update دستی نداشته باشیم.

    //چون منبع حقیقت(Source of Truth) آن‌ها GitHub است، نه کاربر.

    //    بنابراین به جای این Featureها:

    //CreateGitRepository
    //    UpdateGitRepository
    //DeleteGitRepository

    //    فقط این Use Caseها را داشته باشیم:

    //SyncRepositories
    //    GetRepository
    //GetRepositories
    //    SyncIssues
    //GetIssue
    //    GetIssues


    //Aggregate Root مستقل است
    //Issueها به Repository تعلق دارند.
    //Webhookهای GitHub مستقیماً Repository را به‌روزرسانی می‌کنند.
    //    همگام‌سازی Repositoryها بدون بارگذاری کامل Developer انجام می‌شود.
    //    Repositoryها در جستجو، فیلتر و Matching مستقل استفاده خواهند شد.
    public sealed class GitRepository : AuditableEntity<Guid>
    {
        private readonly List<Issue.GitIssue> _issues = new();

        private GitRepository()
        {
        }

        public Guid DeveloperId { get; private set; }

        public string GithubId { get; private set; } = null!;
        public string OwnerLogin { get; private set; } = null!;

        public string Name { get; private set; } = null!;

        public string FullName { get; private set; } = null!;

        public string? Description { get; private set; }

        public string? Language { get; private set; }

        public int Stars { get; private set; }

        public int Forks { get; private set; }
        public bool IsFork { get; private set; }
        public int OpenIssues { get; private set; }
        public bool IsArchived { get; private set; }

        public bool IsPrivate { get; private set; }

        public string Url { get; private set; } = null!;
        public DateTime GithubUpdatedAtUtc { get; private set; }
        public DateTime? LastPushedAtUtc { get; private set; }
        public DateTime LastSyncedAtUtc { get; private set; }
        public DateTime? LastIssuesSyncedAtUtc { get; private set; }
        public Developer.Developer Developer { get; private set; } = null!;

        public IReadOnlyCollection<Issue.GitIssue> Issues
            => _issues.AsReadOnly();
       
        public static GitRepository Create(
            Guid developerId,
            string githubId,
            string name,
            string fullName,
            string? description,
            string? language,
            int stars,
            int forks,
            int openIssues,
            bool isPrivate,
            string url)
        {
            string owner = fullName.Contains('/') ? fullName.Split('/')[0] : string.Empty;
            DateTime now = DateTime.UtcNow;
            return new GitRepository
            {
                Id = Guid.NewGuid(),

                DeveloperId = developerId,

                GithubId = githubId,

                Name = name,

                FullName = fullName,

                Description = description,

                Language = language,

                Stars = stars,

                Forks = forks,

                OpenIssues = openIssues,

                IsPrivate = isPrivate,

                Url = url,

                GithubUpdatedAtUtc = now,
                LastSyncedAtUtc = now,
                CreatedAtUtc = now
            };
        }
        public static GitRepository CreateFromGitHub(
            Guid developerId,
            long githubId,
            string ownerLogin,
            string name,
            string fullName,
            string? description,
            string htmlUrl,
            string? language,
            bool isPrivate,
            bool isFork,
            bool isArchived,
            int stars,
            int forks,
            int openIssues,
            DateTimeOffset githubUpdatedAt,
            DateTimeOffset? lastPushedAt,
            DateTime syncedAtUtc)
        {
            return new GitRepository
            {
                Id = Guid.NewGuid(),
                DeveloperId = developerId,
                GithubId = githubId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                OwnerLogin = ownerLogin,
                Name = name,
                FullName = fullName,
                Description = description,
                Url = htmlUrl,
                Language = language,
                IsPrivate = isPrivate,
                IsFork = isFork,
                IsArchived = isArchived,
                Stars = stars,
                Forks = forks,
                OpenIssues = openIssues,
                GithubUpdatedAtUtc = githubUpdatedAt.UtcDateTime,
                LastPushedAtUtc = lastPushedAt?.UtcDateTime,
                LastSyncedAtUtc = syncedAtUtc,
                CreatedAtUtc = syncedAtUtc
            };
        }
        public void SyncFromGitHub(
            string ownerLogin,
            string name,
            string fullName,
            string? description,
            string htmlUrl,
            string? language,
            bool isPrivate,
            bool isFork,
            bool isArchived,
            int stars,
            int forks,
            int openIssues,
            DateTimeOffset githubUpdatedAt,
            DateTimeOffset? lastPushedAt,
            DateTime syncedAtUtc)
        {
            OwnerLogin = ownerLogin;
            Name = name;
            FullName = fullName;
            Description = description;
            Url = htmlUrl;
            Language = language;
            IsPrivate = isPrivate;
            IsFork = isFork;
            IsArchived = isArchived;
            Stars = stars;
            Forks = forks;
            OpenIssues = openIssues;
            GithubUpdatedAtUtc = githubUpdatedAt.UtcDateTime;
            LastPushedAtUtc = lastPushedAt?.UtcDateTime;
            LastSyncedAtUtc = syncedAtUtc;
            UpdatedAtUtc = syncedAtUtc;
        }

        public void Update(
            string? description,
            string? language,
            int stars,
            int forks,
            int openIssues,
            bool isPrivate)
        {
            Description = description;

            Language = language;

            Stars = stars;

            Forks = forks;

            OpenIssues = openIssues;

            IsPrivate = isPrivate;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void MarkIssuesSynced(DateTime syncedAtUtc)
        {
            LastIssuesSyncedAtUtc = syncedAtUtc;
            UpdatedAtUtc = syncedAtUtc;
        }
    }
}