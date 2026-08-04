using DevMatch.Domain.Entities.Issue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration
{
    public sealed class GitIssueConfiguration : IEntityTypeConfiguration<GitIssue>
    {
        public void Configure(EntityTypeBuilder<GitIssue> builder)
        {
            builder.ToTable("Issues");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Body).HasColumnType("text");
            builder.Property(x => x.Url).IsRequired().HasMaxLength(500);
            builder.Property(x => x.State).HasConversion<int>();
            builder.Property(x => x.Difficulty).HasConversion<int>();

            builder.HasIndex(x => new { x.GitRepositoryId, x.GithubIssueId }).IsUnique();
            builder.HasIndex(x => new { x.State, x.IsAssigned });
            builder.HasIndex(x => x.GithubUpdatedAtUtc);
            builder.HasIndex(x => x.IsGoodFirstIssue);
            builder.HasIndex(x => x.IsHelpWanted);

            builder.HasOne(x => x.GitRepository)
                .WithMany(x => x.Issues)
                .HasForeignKey(x => x.GitRepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}