using DevMatch.Domain.Entities.Developer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class DeveloperGitHubCredentialConfiguration
    : IEntityTypeConfiguration<DeveloperGitHubCredential>
{
    public void Configure(EntityTypeBuilder<DeveloperGitHubCredential> builder)
    {
        builder.ToTable("DeveloperGitHubCredentials");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProtectedAccessToken)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TokenType)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Scopes)
            .HasColumnType("text[]")
            .HasDefaultValue(Array.Empty<string>());

        builder.HasIndex(x => x.DeveloperId).IsUnique();

        builder.HasOne(x => x.Developer)
            .WithOne()
            .HasForeignKey<DeveloperGitHubCredential>(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
