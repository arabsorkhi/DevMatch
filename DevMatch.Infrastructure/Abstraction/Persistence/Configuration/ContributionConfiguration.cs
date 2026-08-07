using DevMatch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class ContributionConfiguration : IEntityTypeConfiguration<Contribution>
{
    public void Configure(EntityTypeBuilder<Contribution> builder)
    {
        builder.ToTable("Contributions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.DeveloperId, x.GitRepositoryId }).IsUnique();

        builder.HasOne(x => x.Developer)
            .WithMany()
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GitRepository)
            .WithMany()
            .HasForeignKey(x => x.GitRepositoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
