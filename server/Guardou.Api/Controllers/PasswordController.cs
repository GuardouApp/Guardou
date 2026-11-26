using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Api.Authorization;
using Guardou.Api.Models;
using Guardou.Application.Authentication;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.PasswordChange)]
[Route("api/users/me/password")]
public sealed class PasswordController(PasswordChanges passwords, ISessionService sessions) : ControllerBase
{
    [HttpPut(Name = "ChangePassword")]
    [EnableRateLimiting(AuthenticationRateLimit.Policy)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Change(PasswordChangeRequest request)
    {
        string userId = User.UserId();
        await passwords.ChangeAsync(userId, request.CurrentPassword, request.NewPassword);
        await sessions.RefreshAsync(userId);
        return NoContent();
    }
}
