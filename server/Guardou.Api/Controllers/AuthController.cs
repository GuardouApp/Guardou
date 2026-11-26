using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Api.Authorization;
using Guardou.Api.Errors;
using Guardou.Api.Models;
using Guardou.Application.Authentication;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Guardou.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ISessionService sessions,
    PasswordLogin passwordLogin,
    Registration registration,
    SignUp signUp,
    UserAccounts accounts,
    IAntiforgery antiforgery)
    : ControllerBase
{
    private const string LockedOutCode = "lockedOut";

    [HttpPost("login", Name = "Login")]
    [EnableRateLimiting(AuthenticationRateLimit.Policy)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        PasswordLoginStatus status = await passwordLogin.SignInAsync(request.Username, request.Password, SessionType.Cookie, request.RememberMe, cancellationToken);
        return ResultOf(status, NoContent());
    }

    [HttpPost("token", Name = "CreateToken")]
    [EnableRateLimiting(AuthenticationRateLimit.Policy)]
    [RequireAntiforgeryToken(required: false)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Token(TokenRequest request, CancellationToken cancellationToken)
    {
        PasswordLoginStatus status = await passwordLogin.SignInAsync(request.Username, request.Password, SessionType.Token, persistent: false, cancellationToken);
        return ResultOf(status, new EmptyResult());
    }

    [HttpPost("refresh", Name = "RefreshToken")]
    [EnableRateLimiting(AuthenticationRateLimit.Policy)]
    [RequireAntiforgeryToken(required: false)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        return await sessions.RefreshTokenAsync(request.RefreshToken)
            ? new EmptyResult()
            : Problem(statusCode: StatusCodes.Status401Unauthorized, title: "The refresh token is not valid.");
    }

    [HttpPost("register", Name = "Register")]
    [EnableRateLimiting(AuthenticationRateLimit.Policy)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        RegistrationResult result = await registration.RegisterFirstAdministratorAsync(request.Username, request.DisplayName, request.Email, request.Password);
        return await ResultOfAsync(result, request, "The registration is closed.");
    }

    [HttpPost("signup", Name = "SignUp")]
    [EnableRateLimiting(AuthenticationRateLimit.Policy)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SignUp(RegisterRequest request, CancellationToken cancellationToken)
    {
        UserProfile profile = new(request.Username, request.DisplayName, request.Email, IsAdmin: false);
        RegistrationResult result = await signUp.RegisterAsync(profile, request.Password, cancellationToken);
        return await ResultOfAsync(result, request, "The sign-up is closed.");
    }

    [Authorize(Policy = Policies.PasswordChange)]
    [HttpPost("logout", Name = "Logout")]
    [ProducesResponseType<LogoutResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<LogoutResult> Logout(CancellationToken cancellationToken)
    {
        return new LogoutResult(await sessions.SignOutAsync(cancellationToken));
    }

    [Authorize(Policy = Policies.PasswordChange)]
    [HttpGet("me", Name = "GetCurrentUser")]
    [ProducesResponseType<CurrentUser>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<CurrentUser> Me()
    {
        UserAccount account = await accounts.GetAsync(User.UserId());
        return new CurrentUser(
            account.Id,
            account.Username,
            account.DisplayName,
            account.Email,
            account.IsAdmin,
            account.HasPicture,
            account.HasPassword,
            account.PasswordChangeRequired);
    }

    [HttpGet("antiforgery", Name = "GetAntiforgeryToken")]
    [ProducesResponseType<AntiforgeryToken>(StatusCodes.Status200OK)]
    public ActionResult<AntiforgeryToken> Antiforgery()
    {
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return new AntiforgeryToken(tokens.RequestToken ?? throw new InvalidOperationException("The antiforgery request token is missing."));
    }

    private IActionResult ResultOf(PasswordLoginStatus status, IActionResult signedIn)
    {
        return status switch
        {
            PasswordLoginStatus.Disabled => Problem(statusCode: StatusCodes.Status403Forbidden, title: "The password login is disabled."),
            PasswordLoginStatus.LockedOut => this.CodedProblem(StatusCodes.Status403Forbidden, "The account is locked. Try again later.", LockedOutCode),
            PasswordLoginStatus.InvalidCredentials => Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid username or password."),
            _ => signedIn,
        };
    }

    private async Task<IActionResult> ResultOfAsync(RegistrationResult result, RegisterRequest request, string closed)
    {
        return result.Status switch
        {
            RegistrationStatus.Closed => Problem(statusCode: StatusCodes.Status403Forbidden, title: closed),
            RegistrationStatus.Rejected => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "The registration is not valid.",
                detail: string.Join(" ", result.Errors)),
            _ => await SignInRegisteredUserAsync(request),
        };
    }

    private async Task<IActionResult> SignInRegisteredUserAsync(RegisterRequest request)
    {
        await sessions.SignInAsync(request.Username, request.Password, SessionType.Cookie, persistent: false);
        return NoContent();
    }
}
