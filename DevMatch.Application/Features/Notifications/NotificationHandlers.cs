using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.Notification;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Notifications;

public sealed record NotificationItem(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);

public sealed class GetNotificationsHandler
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsHandler(IDevMatchDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyCollection<NotificationItem>>> Handle(
        bool unreadOnly,
        int limit,
        CancellationToken cancellationToken)
    {
        limit = Math.Clamp(limit, 1, 100);
        IQueryable<UserNotification> query = _dbContext.UserNotifications
            .AsNoTracking()
            .Where(x => x.DeveloperId == _currentUser.DeveloperId);

        if (unreadOnly)
            query = query.Where(x => !x.IsRead);

        NotificationItem[] items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(limit)
            .Select(x => new NotificationItem(
                x.Id,
                x.Type,
                x.Title,
                x.Message,
                x.IsRead,
                x.CreatedAtUtc,
                x.ReadAtUtc))
            .ToArrayAsync(cancellationToken);

        return Result<IReadOnlyCollection<NotificationItem>>.Success(items);
    }
}

public sealed class MarkNotificationReadHandler
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public MarkNotificationReadHandler(
        IDevMatchDbContext dbContext,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid>> Handle(Guid notificationId, CancellationToken cancellationToken)
    {
        UserNotification? notification = await _dbContext.UserNotifications
            .SingleOrDefaultAsync(
                x => x.Id == notificationId && x.DeveloperId == _currentUser.DeveloperId,
                cancellationToken);

        if (notification is null)
        {
            return Result<Guid>.Failure(
                Error.NotFound("Notifications.NotFound", "The notification was not found."));
        }

        notification.MarkRead(_timeProvider.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(notification.Id);
    }
}
