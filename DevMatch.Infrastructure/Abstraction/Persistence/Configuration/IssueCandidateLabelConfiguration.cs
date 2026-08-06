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
    public sealed class IssueCandidateLabelConfiguration : IEntityTypeConfiguration<IssueCandidateLabel>
    {
        public void Configure(EntityTypeBuilder<IssueCandidateLabel> builder)
        {
            builder.ToTable("IssueCandidateLabel");
            builder.HasKey(x => new { x.IssueCandidateId, x.IssueLabelId });

            builder.HasOne(x => x.IssueCandidate)
                .WithMany(x => x.Labels)
                .HasForeignKey(x => x.IssueCandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.IssueLabel)
                .WithMany(x => x.IssueCandidates)
                .HasForeignKey(x => x.IssueLabelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}