using DevMatch.Domain.Entities.Skill;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedName).IsUnique();
        builder.Navigation(x => x.Aliases).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
