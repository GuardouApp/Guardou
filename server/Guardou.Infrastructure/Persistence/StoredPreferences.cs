namespace Guardou.Infrastructure.Persistence;

internal sealed class StoredPreferences(string userId, string value)
{
    public string UserId { get; private set; } = userId;

    public string Value { get; private set; } = value;

    public void Change(string value)
    {
        Value = value;
    }
}
