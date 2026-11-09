namespace Guardou.Application.Quotas;

internal sealed class StorageReservations(IStorageQuotaStore quotas, StorageQuotaLocks locks)
{
    public async Task<T> ReserveAsync<T>(string ownerId, long bytes, Func<Task<T>> work, CancellationToken cancellationToken)
    {
        SemaphoreSlim ownerLock = locks.For(ownerId);
        await ownerLock.WaitAsync(cancellationToken);
        try
        {
            if (!(await quotas.FindAsync(ownerId, cancellationToken)).Permits(bytes))
            {
                throw new QuotaExceededException();
            }

            return await work();
        }
        finally
        {
            ownerLock.Release();
        }
    }
}
