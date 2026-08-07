using DevMatch.Domain.Entities.DailyRecommendation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class DailyRecommendationConfiguration : IEntityTypeConfiguration<DailyRecommendation>
{
    public void Configure(EntityTypeBuilder<DailyRecommendation> builder)
    {
        builder.ToTable("DailyRecommendations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score).HasPrecision(6, 2);
        builder.Property(x => x.MatchedSkills).HasColumnType("text[]");
        builder.Property(x => x.MissingSkills).HasColumnType("text[]");
        builder.Property(x => x.Reasons).HasColumnType("text[]");
        builder.HasIndex(x => new { x.DeveloperId, x.GeneratedAtUtc });
        builder.HasIndex(x => new { x.DeveloperId, x.IssueId, x.GeneratedAtUtc });

        builder.HasOne<DevMatch.Domain.Entities.Developer.Developer>()
            .WithMany()
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<DevMatch.Domain.Entities.Issue.GitIssue>()
            .WithMany()
            .HasForeignKey(x => x.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
