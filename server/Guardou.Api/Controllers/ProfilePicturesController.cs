using Guardou.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{id}/picture")]
public sealed class ProfilePicturesController(IProfilePictureStore pictures) : ControllerBase
{
    private const string WebpMimeType = "image/webp";
    private const string CachePolicy = "private, no-cache";

    [HttpGet(Name = "GetUserPicture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Get(string id)
    {
        if (pictures.TryOpen(id) is not { } picture)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "The user has no picture.");
        }

        Response.Headers.CacheControl = CachePolicy;
        EntityTagHeaderValue entityTag = new($"\"{picture.Modified.UtcTicks}\"");
        return File(picture.Content, WebpMimeType, picture.Modified, entityTag);
    }
}
