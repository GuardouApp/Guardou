using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public sealed record ClaimedTask(Guid Id, TaskKind Kind);
