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
    public sealed class SkillAliasConfiguration
        : IEntityTypeConfiguration<SkillAlias>
    {
        public void Configure(
            EntityTypeBuilder<SkillAlias> builder)
        {
            builder.ToTable("SkillAliases");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Alias)
                .HasMaxLength(100)
                .IsRequired();
            //هر Skill نمی‌تواند Alias تکراری داشته باشد.
           // ولی یک Alias در صورت نیاز می‌تواند برای Skillهای متفاوت نیز وجود داشته باشد(اگر واقعاً در دامنه کسب‌وکار منطقی باشد).
            builder.HasIndex(x => new
                {
                    x.SkillId,
                    x.Alias
                })
                .IsUnique();

            builder.HasOne(x => x.Skill)
                .WithMany(x => x.Aliases)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
