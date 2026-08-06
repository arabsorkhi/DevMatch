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
    public sealed class RepositoryTopicConfiguration : IEntityTypeConfiguration<RepositoryTopic>
    {
        public void Configure(EntityTypeBuilder<RepositoryTopic> builder)
        {
            builder.ToTable("RepositoryTopic");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => new { x.RepositorySourceId, x.NormalizedName }).IsUnique();
            builder.HasIndex(x => x.IsTargetTechnology);
        }
    }

}
