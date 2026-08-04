using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities;
using DevMatch.Domain.Entities.DailyRecommendation;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Entities.Skill;
using DevMatch.SharedKernel.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Clean Architecture

//         Api
//    /           \
//   /             \
// Application   Infrastructure
//    \            /
//     \          /
//        Domain


//Infrastructure

//    ↓

//Application

//    ↓

//Domain

namespace DevMatch.Infrastructure.Abstraction.Persistence
{
    //حالا DbContext مسئول Audit است.

    public class DevMatchDbContext
        : DbContext, IDevMatchDbContext
    {
        public DevMatchDbContext(
            DbContextOptions<DevMatchDbContext> options)
            : base(options)
        {
        }

        public DbSet<Developer> Developers => Set<Developer>();
        public DbSet<DeveloperGitHubCredential> DeveloperGitHubCredentials => Set<DeveloperGitHubCredential>();

        public DbSet<GitRepository> GitRepositories => Set<GitRepository>();
        public DbSet<GitIssue> GitIssues => Set<GitIssue>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<DeveloperSkill> DeveloperSkills => Set<DeveloperSkill>();
        public DbSet<IssueSkill> IssueSkills => Set<IssueSkill>();
        public DbSet<Contribution> Contributions =>            Set<Contribution>();
        public DbSet<DailyRecommendation>  DailyRecommendations => Set<DailyRecommendation>();
       
        
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DevMatchDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();

            return await base.SaveChangesAsync(
                cancellationToken);
        }

        private void UpdateAuditableEntities()
        {
            var entries =
                ChangeTracker
                    .Entries<AuditableEntity<Guid>>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                }
            }
        }
    }
}
