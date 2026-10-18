using System.ComponentModel.DataAnnotations;
using Guardou.Api.Authorization;
using Guardou.Application.Tasks;
using Guardou.Domain.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Admin)]
[Route("api/tasks")]
public sealed class TasksController(TaskAdministration tasks) : ControllerBase
{
    private const int DefaultLimit = 100;
    private const int MaximumLimit = 500;

    [HttpGet("queues", Name = "ListTaskQueues")]
    [ProducesResponseType<IReadOnlyList<TaskQueueSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<TaskQueueSummary>> Queues(CancellationToken cancellationToken)
    {
        return tasks.QueuesAsync(cancellationToken);
    }

    [HttpGet(Name = "ListTasks")]
    [ProducesResponseType<IReadOnlyList<TaskEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public Task<IReadOnlyList<TaskEntry>> List(
        [FromQuery] TaskKind? kind,
        [FromQuery] TaskState? state,
        [FromQuery][Range(1, MaximumLimit)] int limit = DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        return tasks.ListAsync(kind, state, limit, cancellationToken);
    }

    [HttpPost("{id}/run", Name = "RunTask")]
    [ProducesResponseType<TaskEntry>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Run(string id, CancellationToken cancellationToken)
    {
        TaskEntry? task = await tasks.RunNowAsync(id, cancellationToken);
        return task is null ? NoContent() : Ok(task);
    }

    [HttpPost("{id}/reset", Name = "ResetTask")]
    [ProducesResponseType<TaskEntry>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<TaskEntry> Reset(string id, CancellationToken cancellationToken)
    {
        return tasks.ResetAsync(id, cancellationToken);
    }

    [HttpPost("queues/{kind}/reset", Name = "ResetFailedTasks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetFailed(TaskKind kind, CancellationToken cancellationToken)
    {
        await tasks.ResetFailedAsync(kind, cancellationToken);
        return NoContent();
    }

    [HttpPost("queues/{kind}/all", Name = "QueueAllTasks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QueueAll(TaskKind kind, CancellationToken cancellationToken)
    {
        await tasks.EnqueueAllAsync(kind, cancellationToken);
        return NoContent();
    }
}
