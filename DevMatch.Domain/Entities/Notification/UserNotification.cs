using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Notification;

public enum NotificationType
{
    NewRecommendations = 1,
    System = 2
}

public sealed class UserNotification : AuditableEntity<Guid>
{
    private UserNotification()
    {
    }

    public Guid DeveloperId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }

    public static UserNotification Create(
        Guid developerId,
        NotificationType type,
        string title,
        string message,
        DateTimeOffset utcNow)
    {
        if (developerId == Guid.Empty)
            throw new ArgumentException("Developer id cannot be empty.", nameof(developerId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Notification title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Notification message is required.", nameof(message));

        DateTimeOffset normalized = utcNow.ToUniversalTime();
        return new UserNotification
        {
            Id = Guid.NewGuid(),
            DeveloperId = developerId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            CreatedAtUtc = normalized
        };
    }

    public void MarkRead(DateTimeOffset utcNow)
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAtUtc = utcNow.ToUniversalTime();
        UpdatedAtUtc = utcNow.ToUniversalTime();
    }
}
