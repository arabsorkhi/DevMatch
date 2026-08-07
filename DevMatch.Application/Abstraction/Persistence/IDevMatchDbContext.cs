using DevMatch.Domain.Entities;
using DevMatch.Domain.Entities.Authentication;
using DevMatch.Domain.Entities.DailyRecommendation;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Entities.Notification;
using DevMatch.Domain.Entities.RecommendationFeedback;
using DevMatch.Domain.Entities.Skill;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Abstraction.Persistence
{
    public interface IDevMatchDbContext
    {

        DbSet<Developer> Developers { get; }
        DbSet<GitRepository> GitRepositories { get; }
        DbSet<GitIssue> GitIssues { get; }
        DbSet<Skill> Skills { get; }
        DbSet<DeveloperSkill> DeveloperSkills { get; }
        DbSet<IssueSkill> IssueSkills { get; }
        DbSet<DailyRecommendation> DailyRecommendations { get; }
        DbSet<Contribution> Contributions { get; }
        DbSet<DeveloperGitHubCredential> DeveloperGitHubCredentials { get; }
        DbSet<GitHubCredential> GitHubCredentials { get; }
        DbSet<DeveloperPreference> DeveloperPreferences { get; }
        DbSet<RecommendationFeedback> RecommendationFeedback { get; }
        DbSet<UserNotification> UserNotifications { get; }

    }
}
