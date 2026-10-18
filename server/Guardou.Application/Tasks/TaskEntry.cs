using Guardou.Domain.Tasks;

namespace Guardou.Application.Tasks;

public sealed record TaskEntry(
    string Id,
    TaskKind Kind,
    TaskState State,
    TaskPriority Priority,
    string ItemId,
    string ItemName,
    string Owner,
    string? Error,
    DateTimeOffset Created,
    DateTimeOffset Updated);
