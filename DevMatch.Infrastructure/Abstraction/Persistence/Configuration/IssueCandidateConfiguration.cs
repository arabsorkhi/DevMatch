using DevMatch.Domain.Entities.Issue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration
{

    public sealed class IssueCandidateConfiguration : IEntityTypeConfiguration<IssueCandidate>
    {
        public void Configure(EntityTypeBuilder<IssueCandidate> builder)
        {
            builder.ToTable("IssueCandidate");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Body).HasMaxLength(30000);
            builder.Property(x => x.HtmlUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.AuthorLogin).HasMaxLength(100);
            builder.Property(x => x.DifficultyScore).HasPrecision(6, 2);
            builder.Property(x => x.CandidateScore).HasPrecision(6, 2);

            builder.HasIndex(x => x.GitHubIssueId).IsUnique();
            builder.HasIndex(x => new { x.RepositorySourceId, x.Number }).IsUnique();
            builder.HasIndex(x => new { x.IsInControlledSet, x.IsEligible, x.State, x.CandidateScore });
            builder.HasIndex(x => x.GitHubUpdatedAt);
        }
    }

}
