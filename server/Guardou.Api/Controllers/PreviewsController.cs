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
    private const string WebpMimeType = "image/webp";
    private const string CachePolicy = "private, max-age=31536000, immutable";
    private const string RetrySeconds = "2";

    [HttpGet("{id}/preview", Name = "GetItemPreview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id, [FromQuery] PreviewSize size, CancellationToken cancellationToken)
    {
        Stream? preview = await previews.OpenAsync(User.UserId(), id, size, cancellationToken);
        if (preview is null)
        {
            Response.Headers.RetryAfter = RetrySeconds;
            return Accepted();
        }

        Response.Headers.CacheControl = CachePolicy;
        return File(preview, WebpMimeType);
    }
}
