using Guardou.Api.Authentication;
using Guardou.Application.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class RecentItemsController(RecentItems recent) : ControllerBase
{
    [HttpGet("recent", Name = "GetRecentItems")]
    [ProducesResponseType<IReadOnlyList<ItemEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IReadOnlyList<ItemEntry>> GetRecent(CancellationToken cancellationToken)
    {
        return recent.ListAsync(User.UserId(), cancellationToken);
    }
}
