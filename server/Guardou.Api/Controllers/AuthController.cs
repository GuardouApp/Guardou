using System.Net.Mime;
using System.Security.Claims;
using Guardou.Api.Authentication;
using Guardou.Api.Models;
using Guardou.Application.Authentication;
using Guardou.Application.Users;
using Guardou.Domain.Users;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISessionService sessions, Registration registration, IAntiforgery antiforgery)
    : ControllerBase
{
    [HttpPost("login", Name = "Login")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!await sessions.SignInAsync(request.Username, request.Password, request.RememberMe))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password.");
        }

        return NoContent();
    }

    [HttpPost("register", Name = "Register")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        RegistrationResult result = await registration.RegisterFirstAdministratorAsync(request.Username, request.Email, request.Password);
        return result.Status switch
        {
            RegistrationStatus.Closed => Problem(statusCode: StatusCodes.Status403Forbidden, title: "The registration is closed."),
            RegistrationStatus.Rejected => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "The registration is not valid.",
                detail: string.Join(" ", result.Errors)),
            _ => await SignInRegisteredUserAsync(request),
        };
    }

    [Authorize]
    [HttpPost("logout", Name = "Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await sessions.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me", Name = "GetCurrentUser")]
    [ProducesResponseType<CurrentUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUser> Me()
    {
        string? username = User.Identity?.Name;
        string? email = User.FindFirstValue(ClaimTypes.Email);
        if (username is null || email is null)
        {
            return Unauthorized();
        }

        return new CurrentUser(User.UserId(), username, email, User.IsInRole(Roles.Admin));
    }

    [HttpGet("antiforgery", Name = "GetAntiforgeryToken")]
    [ProducesResponseType<AntiforgeryToken>(StatusCodes.Status200OK)]
    public ActionResult<AntiforgeryToken> Antiforgery()
    {
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return new AntiforgeryToken(tokens.RequestToken ?? throw new InvalidOperationException("The antiforgery request token is missing."));
    }

    private async Task<IActionResult> SignInRegisteredUserAsync(RegisterRequest request)
    {
        await sessions.SignInAsync(request.Username, request.Password, persistent: false);
        return NoContent();
    }
}
