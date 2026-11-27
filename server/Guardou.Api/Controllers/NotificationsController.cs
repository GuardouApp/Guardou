using Guardou.Api.Authentication;
using Guardou.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController(NotificationCenter notifications) : ControllerBase
{
    [HttpGet(Name = "ListNotifications")]
    [ProducesResponseType<IReadOnlyList<NotificationEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IReadOnlyList<NotificationEntry>> List(CancellationToken cancellationToken)
    {
        return notifications.ListAsync(User.UserId(), cancellationToken);
    }

    [HttpPost("read", Name = "MarkNotificationsRead")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkRead(CancellationToken cancellationToken)
    {
        await notifications.MarkAllReadAsync(User.UserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete(Name = "ClearNotifications")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await notifications.ClearAsync(User.UserId(), cancellationToken);
        return NoContent();
    }
}
