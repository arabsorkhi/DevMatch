using DevMatch.Domain.Entities.Issue;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration
{

    public sealed class RepositorySourceConfiguration : IEntityTypeConfiguration<RepositorySource>
    {
        public void Configure(EntityTypeBuilder<RepositorySource> builder)
        {
            builder.ToTable("RepositorySource");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Owner).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(220).IsRequired();
            builder.Property(x => x.HtmlUrl).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.PrimaryLanguage).HasMaxLength(100);
            builder.Property(x => x.SelectionReason).HasMaxLength(1000);
            builder.Property(x => x.QualityScore).HasPrecision(6, 2);
            builder.Property(x => x.MaintainerResponseRate).HasPrecision(5, 4);

            builder.HasIndex(x => x.GitHubRepositoryId).IsUnique();
            builder.HasIndex(x => x.FullName).IsUnique();
            builder.HasIndex(x => new { x.SelectionStatus, x.IsEnabled, x.QualityScore });

            builder.HasMany(x => x.Topics)
                .WithOne(x => x.RepositorySource)
                .HasForeignKey(x => x.RepositorySourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Labels)
                .WithOne(x => x.RepositorySource)
                .HasForeignKey(x => x.RepositorySourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Issues)
                .WithOne(x => x.RepositorySource)
                .HasForeignKey(x => x.RepositorySourceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SyncState)
                .WithOne(x => x.RepositorySource)
                .HasForeignKey<IssueSyncState>(x => x.RepositorySourceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
