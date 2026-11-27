using Guardou.Application.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Guardou.Infrastructure.Persistence;

internal sealed class EfWriteTransactions(AppDbContext context, ItemChangeOutbox outbox) : IWriteTransactions
{
    public async Task<T> RunAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        T result = await work();
        await transaction.CommitAsync(cancellationToken);
        await outbox.FlushAsync(cancellationToken);
        return result;
    }
}
