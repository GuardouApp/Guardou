using Guardou.Api.Authentication;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/search")]
public sealed class UserSearchController(UserSearch search) : ControllerBase
{
    [HttpGet(Name = "SearchUsers")]
    [ProducesResponseType<IReadOnlyList<UserSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<IReadOnlyList<UserSummary>> Search([FromQuery] string? query, CancellationToken cancellationToken)
    {
        return search.SearchAsync(User.UserId(), query, cancellationToken);
    }
}
