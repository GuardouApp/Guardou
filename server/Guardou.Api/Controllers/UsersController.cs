using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Api.Authorization;
using Guardou.Api.Models;
using Guardou.Application.Authentication;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Admin)]
[Route("api/users")]
public sealed class UsersController(UserAccounts accounts, ISessionService sessions) : ControllerBase
{
    [HttpGet(Name = "ListUsers")]
    [ProducesResponseType<IReadOnlyList<UserAccount>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<UserAccount>> List()
    {
        return accounts.ListAsync();
    }

    [HttpGet("{id}", Name = "GetUser")]
    [ProducesResponseType<UserAccount>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<UserAccount> Get(string id)
    {
        return accounts.GetAsync(id);
    }

    [HttpPost(Name = "CreateUser")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<UserAccount>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(UserRequest request)
    {
        UserAccount account = await accounts.CreateAsync(ProfileOf(request), request.Password);
        return CreatedAtRoute("GetUser", new { id = account.Id }, account);
    }

    [HttpPut("{id}", Name = "UpdateUser")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<UserAccount>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<UserAccount> Update(string id, UserRequest request)
    {
        UserAccount account = await accounts.UpdateAsync(id, ProfileOf(request), request.Password);
        if (account.Id == User.UserId())
        {
            await sessions.RefreshAsync(account.Id);
        }

        return account;
    }

    [HttpDelete("{id}", Name = "DeleteUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        await accounts.DeleteAsync(User.UserId(), id, cancellationToken);
        return NoContent();
    }

    private static UserProfile ProfileOf(UserRequest request)
    {
        return new UserProfile(request.Username, request.Email, request.IsAdmin);
    }
}
