using Guardou.Api.Authentication;
using Guardou.Application.Previews;
using Guardou.Domain.Previews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class PreviewsController(ItemPreviews previews) : ControllerBase
{
    private const string WebpMimeType = "image/webp";
    private const string CurrentCachePolicy = "private, no-cache";
    private const string FixedCachePolicy = "private, max-age=31536000, immutable";
    private const string RetrySeconds = "2";

    [HttpGet("{id}/preview", Name = "GetItemPreview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent(string id, [FromQuery] PreviewSize size, CancellationToken cancellationToken)
    {
        CachedPreview? preview = await previews.OpenCurrentAsync(User.UserId(), id, size, cancellationToken);
        if (preview is null)
        {
            Response.Headers.RetryAfter = RetrySeconds;
            return Accepted();
        }

        return Preview(preview, size, CurrentCachePolicy);
    }

    [HttpGet("{id}/previews/{previewId}", Name = "GetItemPreviewById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        string id,
        string previewId,
        [FromQuery] PreviewSize size,
        CancellationToken cancellationToken)
    {
        CachedPreview preview = await previews.OpenAsync(User.UserId(), id, previewId, size, cancellationToken);
        return Preview(preview, size, FixedCachePolicy);
    }

    private FileStreamResult Preview(CachedPreview preview, PreviewSize size, string cachePolicy)
    {
        Response.Headers.CacheControl = cachePolicy;
        EntityTagHeaderValue entityTag = new($"\"{preview.PreviewId}-{size.ToString().ToLowerInvariant()}\"");
        return File(preview.Content, WebpMimeType, lastModified: null, entityTag);
    }
}
