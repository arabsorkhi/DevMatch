using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Developer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration
{
    public class DeveloperConfiguration
        : IEntityTypeConfiguration<Developer>
    {
        public void Configure(
            EntityTypeBuilder<Developer> builder)
        {
            builder.ToTable("Developers");

            builder.HasKey(x => x.Id);
            builder.HasQueryFilter(
                x => !x.IsDeleted);
            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAtUtc);

            builder.Property(x => x.UserName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.GitHubUserId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.DisplayName)
                .HasMaxLength(150);

            builder.Property(x => x.Email)
                .HasMaxLength(200);

            builder.Property(x => x.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Bio)
                .HasMaxLength(500);

            builder.Property(x => x.Location)
                .HasMaxLength(150);

            builder.HasIndex(x => x.GitHubUserId)
                .IsUnique();

            builder.HasIndex(x => x.UserName)
                .IsUnique();
        }
    }
}
