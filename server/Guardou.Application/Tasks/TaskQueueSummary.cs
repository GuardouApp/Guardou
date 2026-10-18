using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public sealed record TaskQueueSummary(TaskKind Kind, int Pending, int Running, int Failed);
