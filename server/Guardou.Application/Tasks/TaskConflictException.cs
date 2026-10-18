namespace Guardou.Application.Tasks;

public sealed class TaskConflictException(string id) : Exception($"The task '{id}' is not pending or failed.");
