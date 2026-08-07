using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class DeveloperSkillConfiguration : IEntityTypeConfiguration<DeveloperSkill>
{
    public void Configure(EntityTypeBuilder<DeveloperSkill> builder)
    {
        builder.ToTable("DeveloperSkills");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.DeveloperId, x.SkillId }).IsUnique();

        builder.Property(x => x.Level).HasConversion<int>();
        builder.Property(x => x.Source).HasConversion<int>();
        builder.Property(x => x.Confidence)
            .HasConversion(
                score => score.Value,
                value => ConfidenceScore.Create(value));

        builder.HasOne(x => x.Developer)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
            .WithMany()
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
