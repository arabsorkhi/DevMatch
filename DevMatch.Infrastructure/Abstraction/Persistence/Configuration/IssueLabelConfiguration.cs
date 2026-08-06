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
    public sealed class IssueLabelConfiguration : IEntityTypeConfiguration<IssueLabel>
    {
        public void Configure(EntityTypeBuilder<IssueLabel> builder)
        {
            builder.ToTable("IssueLabel");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
            builder.Property(x => x.NormalizedName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Color).HasMaxLength(20);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.HasIndex(x => new { x.RepositorySourceId, x.NormalizedName }).IsUnique();
            builder.HasIndex(x => x.GitHubLabelId);
        }
    }

}
