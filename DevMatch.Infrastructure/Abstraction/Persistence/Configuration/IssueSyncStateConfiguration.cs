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
    public sealed class IssueSyncStateConfiguration : IEntityTypeConfiguration<IssueSyncState>
    {
        public void Configure(EntityTypeBuilder<IssueSyncState> builder)
        {
            builder.ToTable("IssueSyncState");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.LeaseOwner).HasMaxLength(200);
            builder.Property(x => x.ETag).HasMaxLength(500);
            builder.Property(x => x.LastError).HasMaxLength(4000);
            builder.HasIndex(x => x.RepositorySourceId).IsUnique();
            builder.HasIndex(x => new { x.Status, x.NextSyncAt });
            builder.HasIndex(x => x.LeaseUntil);
        }
    }
}
