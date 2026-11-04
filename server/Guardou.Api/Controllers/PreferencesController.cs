using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Application.Preferences;
using Guardou.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/me/preferences")]
public sealed class PreferencesController(UserPreferencesService preferences) : ControllerBase
{
    [HttpGet(Name = "GetPreferences")]
    [ProducesResponseType<UserPreferences>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<UserPreferences> Get(CancellationToken cancellationToken)
    {
        return preferences.GetAsync(User.UserId(), cancellationToken);
    }

    [HttpGet("defaults", Name = "GetPreferencesDefaults")]
    [ProducesResponseType<UserPreferences>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public UserPreferences GetDefaults()
    {
        return UserPreferences.Defaults;
    }

    [HttpPut(Name = "ChangePreferences")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<UserPreferences>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public Task<UserPreferences> Change(UserPreferences changed, CancellationToken cancellationToken)
    {
        return preferences.ChangeAsync(User.UserId(), changed, cancellationToken);
    }
}
