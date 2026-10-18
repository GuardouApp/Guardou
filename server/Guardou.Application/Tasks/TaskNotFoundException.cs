namespace Guardou.Application.Tasks;

public sealed class TaskNotFoundException(string id) : Exception($"The task '{id}' does not exist.");
