using System.Net.Mime;
using Guardou.Api.Authorization;
using Guardou.Application.Authentication;
using Guardou.Application.Configuration;
using Guardou.Application.Settings;
using Guardou.Application.Tasks;
using Guardou.Domain.Settings;
using Guardou.Infrastructure.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Admin)]
[Route("api/settings")]
public sealed class SettingsController(
    QueueSettings queues,
    OidcSettingsAdministration oidc,
    ServerSettingsAdministration server,
    EffectiveConfigurationQuery effective) : ControllerBase
{
    [HttpGet("tasks", Name = "GetTaskSettings")]
    [ProducesResponseType<IReadOnlyList<TaskConcurrency>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<TaskConcurrency>> GetTasks(CancellationToken cancellationToken)
    {
        return queues.ConcurrencyAsync(cancellationToken);
    }

    [HttpGet("tasks/defaults", Name = "GetTaskSettingsDefaults")]
    [ProducesResponseType<IReadOnlyList<TaskConcurrency>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public IReadOnlyList<TaskConcurrency> GetTaskDefaults()
    {
        return queues.DefaultConcurrency();
    }

    [HttpPut("tasks", Name = "ChangeTaskSettings")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<IReadOnlyList<TaskConcurrency>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IReadOnlyList<TaskConcurrency>> ChangeTasks(
        IReadOnlyList<TaskConcurrency> settings,
        CancellationToken cancellationToken)
    {
        await queues.ChangeConcurrencyAsync(settings, cancellationToken);
        return await queues.ConcurrencyAsync(cancellationToken);
    }

    [HttpGet("oidc", Name = "GetOidcSettings")]
    [ProducesResponseType<OidcSettingsView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<OidcSettingsView> GetOidc(CancellationToken cancellationToken)
    {
        return oidc.GetAsync(Request.Origin(), cancellationToken);
    }

    [HttpGet("oidc/defaults", Name = "GetOidcSettingsDefaults")]
    [ProducesResponseType<OidcSettingsView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<OidcSettingsView> GetOidcDefaults(CancellationToken cancellationToken)
    {
        return oidc.DefaultsAsync(Request.Origin(), cancellationToken);
    }

    [HttpPut("oidc", Name = "ChangeOidcSettings")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<OidcSettingsView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<OidcSettingsView> ChangeOidc(OidcSettingsChange settings, CancellationToken cancellationToken)
    {
        return oidc.ChangeAsync(settings, Request.Origin(), cancellationToken);
    }

    [HttpGet("server", Name = "GetServerSettings")]
    [ProducesResponseType<ServerSettingsView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<ServerSettingsView> GetServer(CancellationToken cancellationToken)
    {
        return server.GetAsync(cancellationToken);
    }

    [HttpGet("server/defaults", Name = "GetServerSettingsDefaults")]
    [ProducesResponseType<ServerSettingsView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public ServerSettingsView GetServerDefaults()
    {
        return server.Defaults();
    }

    [HttpPut("server", Name = "ChangeServerSettings")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ServerSettingsView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<ServerSettingsView> ChangeServer(ServerSettings settings, CancellationToken cancellationToken)
    {
        return server.ChangeAsync(settings, cancellationToken);
    }

    [HttpGet("effective", Name = "GetEffectiveConfiguration")]
    [ProducesResponseType<IReadOnlyList<EffectiveSetting>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<EffectiveSetting>> GetEffective(CancellationToken cancellationToken)
    {
        return effective.GetAsync(cancellationToken);
    }
}
