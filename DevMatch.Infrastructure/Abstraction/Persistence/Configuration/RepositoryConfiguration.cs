using DevMatch.Domain.Entities.GitRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration
{
    public sealed class RepositoryConfiguration
        : IEntityTypeConfiguration<GitRepository>
    {
        public void Configure(
            EntityTypeBuilder<GitRepository> builder)
        {
            builder.ToTable("Repositories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GithubId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.Language)
                .HasMaxLength(100);

            builder.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(x => x.GithubId)
                .IsUnique();

            builder.HasOne(x => x.Developer)
                .WithMany(x => x.Repositories)
                .HasForeignKey(x => x.DeveloperId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.Issues)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
