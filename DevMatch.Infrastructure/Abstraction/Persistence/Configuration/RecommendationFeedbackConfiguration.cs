using DevMatch.Domain.Entities.RecommendationFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class RecommendationFeedbackConfiguration : IEntityTypeConfiguration<RecommendationFeedback>
{
    public void Configure(EntityTypeBuilder<RecommendationFeedback> builder)
    {
        builder.ToTable("RecommendationFeedback");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Outcome).HasConversion<int>();
        builder.HasIndex(x => new { x.DeveloperId, x.IssueId }).IsUnique();
        builder.HasIndex(x => new { x.DeveloperId, x.Outcome, x.OccurredAtUtc });

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
