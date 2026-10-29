namespace Guardou.Application.Shares;

public interface IShareLookup
{
    Task<IReadOnlySet<Guid>> SharedAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken);
}
