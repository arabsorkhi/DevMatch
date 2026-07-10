using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.GitRepository
{
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

        public string Name { get; private set; } = null!;

        public string FullName { get; private set; } = null!;

        public string? Description { get; private set; }

        public string? Language { get; private set; }

        public int Stars { get; private set; }

        public int Forks { get; private set; }

        public int OpenIssues { get; private set; }

        public bool IsPrivate { get; private set; }

        public string Url { get; private set; } = null!;

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

                CreatedAtUtc = DateTime.UtcNow
            };
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
        //public Repository.Repository AddRepository(
        //    string githubId,
        //    string name,
        //    string fullName,
        //    string? description,
        //    string? language,
        //    int stars,
        //    int forks,
        //    int openIssues,
        //    bool isPrivate,
        //    string url)
        //{
        //    var repository = Repository.Repository.Create(
        //        Id,
        //        githubId,
        //        name,
        //        fullName,
        //        description,
        //        language,
        //        stars,
        //        forks,
        //        openIssues,
        //        isPrivate,
        //        url);

        //    _repositories.Add(repository);

        //    return repository;
        //}
    }
}