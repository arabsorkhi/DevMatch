using DevMatch.Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class GitHubCredentialConfiguration : IEntityTypeConfiguration<GitHubCredential>
{
    public void Configure(EntityTypeBuilder<GitHubCredential> builder)
    {
        builder.ToTable("GitHubCredentials");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DeveloperId).IsUnique();

        builder.Property(x => x.ProtectedAccessToken).HasColumnType("text").IsRequired();
        builder.Property(x => x.TokenType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Scope).HasMaxLength(1_000);

        builder.HasOne(x => x.Developer)
            .WithOne()
            .HasForeignKey<GitHubCredential>(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
