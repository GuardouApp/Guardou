namespace Guardou.Domain.Settings;

public interface ISettings<TSelf>
    where TSelf : ISettings<TSelf>
{
    static abstract string Key { get; }

    static abstract TSelf Initial { get; }
}
