namespace Guardou.Application.Files;

public interface IUserNames
{
    Task<IReadOnlyDictionary<string, string>> DisplayNamesAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken);
}
