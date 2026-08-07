using DevMatch.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevMatch.Infrastructure.Abstraction.Persistence.Configuration;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1_000).IsRequired();
        builder.HasIndex(x => new { x.DeveloperId, x.IsRead, x.CreatedAtUtc });

        builder.HasOne<DevMatch.Domain.Entities.Developer.Developer>()
            .WithMany()
            .HasForeignKey(x => x.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
