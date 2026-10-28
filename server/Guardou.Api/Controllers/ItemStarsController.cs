using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Api.Models;
using Guardou.Application.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class ItemStarsController(ItemStars stars) : ControllerBase
{
    [HttpGet("starred", Name = "GetStarredItems")]
    [ProducesResponseType<IReadOnlyList<ItemEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IReadOnlyList<ItemEntry>> GetStarred(CancellationToken cancellationToken)
    {
        return stars.ListAsync(User.UserId(), cancellationToken);
    }

    [HttpPost("star", Name = "StarItems")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Star(ItemSelection selection, CancellationToken cancellationToken)
    {
        await stars.StarAsync(User.UserId(), selection.Ids, cancellationToken);
        return NoContent();
    }

    [HttpPost("unstar", Name = "UnstarItems")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unstar(ItemSelection selection, CancellationToken cancellationToken)
    {
        await stars.UnstarAsync(User.UserId(), selection.Ids, cancellationToken);
        return NoContent();
    }
}
