using Guardou.Domain.Users;

namespace Guardou.Application.Quotas;

public interface IStorageQuotaStore
{
    Task<StorageQuota> FindAsync(string ownerId, CancellationToken cancellationToken);
}
