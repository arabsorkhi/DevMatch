using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Notifications;

namespace DevMatch.Api.Endpoints.Notifications;

public sealed class NotificationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications", GetAsync)
            .RequireAuthorization()
            .WithTags("Notifications");

        app.MapPost("/api/notifications/{notificationId:guid}/read", MarkReadAsync)
            .RequireAuthorization()
            .WithTags("Notifications");
    }

    private static async Task<IResult> GetAsync(
        bool? unreadOnly,
        int? limit,
        GetNotificationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            unreadOnly ?? false,
            limit.GetValueOrDefault(30),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> MarkReadAsync(
        Guid notificationId,
        MarkNotificationReadHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(notificationId, cancellationToken);
        return result.IsSuccess ? Results.Ok(new { notificationId = result.Value }) : result.ToProblem();
    }
}
