using DevMatch.Domain.Entities.Developer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class DeveloperPreferenceConfiguration : IEntityTypeConfiguration<DeveloperPreference>
{
    public void Configure(EntityTypeBuilder<DeveloperPreference> builder)
    {
        builder.ToTable("DeveloperPreferences");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DeveloperId).IsUnique();

        builder.Property(x => x.SelfReportedLevel).HasConversion<int>();
        builder.Property(x => x.PreferredLanguages).HasColumnType("text[]");
        builder.Property(x => x.PreferredTopics).HasColumnType("text[]");
        builder.Property(x => x.ExcludedLabels).HasColumnType("text[]");

        builder.HasOne(x => x.Developer)
            .WithOne()
            .HasForeignKey<DeveloperPreference>(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
