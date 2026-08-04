using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Skill;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration
{
    public sealed class IssueSkillConfiguration
        : IEntityTypeConfiguration<IssueSkill>
    {
        public void Configure(
            EntityTypeBuilder<IssueSkill> builder)
        {
            builder.HasKey(issueSkill => new
            {
                issueSkill.GitIssueId,
                issueSkill.SkillId
            });

            builder.HasOne(issueSkill =>
                    issueSkill.GitIssue)
                .WithMany(issue =>
                    issue.IssueSkills)
                .HasForeignKey(issueSkill =>
                    issueSkill.GitIssueId);

            builder.HasOne(issueSkill =>
                    issueSkill.Skill)
                .WithMany()
                .HasForeignKey(issueSkill =>
                    issueSkill.SkillId);
        }
    }
}
