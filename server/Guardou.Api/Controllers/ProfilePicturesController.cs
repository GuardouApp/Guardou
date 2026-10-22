using System.Net.Mime;
using Guardou.Api.ActionResults;
using Guardou.Api.Authentication;
using Guardou.Api.Models;
using Guardou.Application.Users;
using Guardou.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{id}/picture")]
public sealed class ProfilePicturesController(IProfilePictureStore store, ProfilePictures pictures) : ControllerBase
{
    private const long MultipartOverheadBytes = 64 * 1024;

    [HttpGet(Name = "GetUserPicture")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Get(string id)
    {
        return this.Picture(store.TryOpen(id));
    }

    [HttpPut(Name = "ReplaceUserPicture")]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [RequestSizeLimit(ProfilePictureLimits.MaximumBytes + MultipartOverheadBytes)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> Replace(string id, [FromForm] PictureForm form, CancellationToken cancellationToken)
    {
        if (!CanChange(id))
        {
            return Forbidden();
        }

        await using Stream content = form.File.OpenReadStream();
        await pictures.ReplaceAsync(id, content, cancellationToken);
        return NoContent();
    }

    [HttpDelete(Name = "DeleteUserPicture")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        if (!CanChange(id))
        {
            return Forbidden();
        }

        await pictures.DeleteAsync(id);
        return NoContent();
    }

    private bool CanChange(string id)
    {
        return id == User.UserId() || User.IsInRole(Roles.Admin);
    }

    private ObjectResult Forbidden()
    {
        return Problem(statusCode: StatusCodes.Status403Forbidden, title: "You cannot change the picture of this user.");
    }
}
