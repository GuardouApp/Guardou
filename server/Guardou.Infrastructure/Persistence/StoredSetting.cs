namespace Guardou.Infrastructure.Persistence;

internal sealed class StoredSetting(string key, string value)
{
    public string Key { get; private set; } = key;

    public string Value { get; private set; } = value;

    public void Change(string value)
    {
        Value = value;
    }
}
