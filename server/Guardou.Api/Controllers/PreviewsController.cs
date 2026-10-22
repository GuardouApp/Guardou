using Guardou.Api.ActionResults;
using Guardou.Api.Authentication;
using Guardou.Application.Previews;
using Guardou.Domain.Previews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class PreviewsController(ItemPreviews previews) : ControllerBase
{
    [HttpGet("{id}/preview", Name = "GetItemPreview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id, [FromQuery] PreviewSize size, CancellationToken cancellationToken)
    {
        return this.Preview(await previews.OpenAsync(User.UserId(), id, size, cancellationToken));
    }
}
