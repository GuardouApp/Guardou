using System.Net.Mime;
using Guardou.Api.Authorization;
using Guardou.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Admin)]
[Route("api/settings")]
public sealed class SettingsController(QueueSettings queues) : ControllerBase
{
    [HttpGet("tasks", Name = "GetTaskSettings")]
    [ProducesResponseType<IReadOnlyList<TaskConcurrency>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<TaskConcurrency>> GetTasks(CancellationToken cancellationToken)
    {
        return queues.ConcurrencyAsync(cancellationToken);
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
}
