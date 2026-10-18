using System.ComponentModel.DataAnnotations;
using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public sealed record TaskConcurrency(
    TaskKind Kind,
    [Range(TaskQueueSettings.MinimumConcurrency, TaskQueueSettings.MaximumConcurrency)] int Concurrency);
