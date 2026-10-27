using Guardou.Api.Authentication;
using Guardou.Application.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class ItemOpensController(ItemOpenRecorder opens) : ControllerBase
{
    [HttpPut("{id}/opened", Name = "RecordItemOpen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Record(string id, CancellationToken cancellationToken)
    {
        await opens.RecordAsync(User.UserId(), id, cancellationToken);
        return NoContent();
    }
}
