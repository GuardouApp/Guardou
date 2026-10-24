using Guardou.Api.Authentication;
using Guardou.Application.Files;
using Guardou.Application.Shares;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class DirectSharesController(DirectSharing sharing) : ControllerBase
{
    private const string RecipientsRoute = "{id}/recipients";
    private const string RecipientRoute = RecipientsRoute + "/{userId}";

    [HttpGet("shared", Name = "GetSharedWithMe")]
    [ProducesResponseType<IReadOnlyList<ItemEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IReadOnlyList<ItemEntry>> GetSharedWithMe(CancellationToken cancellationToken)
    {
        return sharing.SharedWithAsync(User.UserId(), cancellationToken);
    }

    [HttpGet(RecipientsRoute, Name = "GetItemRecipients")]
    [ProducesResponseType<IReadOnlyList<UserSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IReadOnlyList<UserSummary>> GetRecipients(string id, CancellationToken cancellationToken)
    {
        return sharing.ListRecipientsAsync(User.UserId(), id, cancellationToken);
    }

    [HttpPut(RecipientRoute, Name = "AddItemRecipient")]
    [ProducesResponseType<IReadOnlyList<UserSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IReadOnlyList<UserSummary>> AddRecipient(string id, string userId, CancellationToken cancellationToken)
    {
        return sharing.AddRecipientAsync(User.UserId(), id, userId, cancellationToken);
    }

    [HttpDelete(RecipientRoute, Name = "RemoveItemRecipient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRecipient(string id, string userId, CancellationToken cancellationToken)
    {
        await sharing.RemoveRecipientAsync(User.UserId(), id, userId, cancellationToken);
        return NoContent();
    }
}
