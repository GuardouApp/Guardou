using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Api.Models;
using Guardou.Application.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/trash")]
public sealed class TrashController(TrashService trash) : ControllerBase
{
    [HttpGet(Name = "GetTrash")]
    [ProducesResponseType<IReadOnlyList<TrashEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IReadOnlyList<TrashEntry>> List(CancellationToken cancellationToken)
    {
        return trash.ListAsync(User.UserId(), cancellationToken);
    }

    [HttpPost("restore", Name = "RestoreItems")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<IReadOnlyList<ItemEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IReadOnlyList<ItemEntry>> Restore(ItemSelection selection, CancellationToken cancellationToken)
    {
        return trash.RestoreAsync(User.UserId(), selection.Ids, cancellationToken);
    }

    [HttpPost("delete", Name = "DeleteItems")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ItemSelection selection, CancellationToken cancellationToken)
    {
        await trash.DeleteAsync(User.UserId(), selection.Ids, cancellationToken);
        return NoContent();
    }

    [HttpDelete(Name = "EmptyTrash")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EmptyTrash(CancellationToken cancellationToken)
    {
        await trash.EmptyAsync(User.UserId(), cancellationToken);
        return NoContent();
    }
}
