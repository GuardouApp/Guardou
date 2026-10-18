namespace Guardou.Application.Files;

public sealed class ItemConflictException(string name)
    : Exception($"An item with the name '{name}' already exists in the destination.")
{
    public string Name { get; } = name;
}
