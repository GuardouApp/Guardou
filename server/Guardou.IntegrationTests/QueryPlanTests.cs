using System.Text.RegularExpressions;
using Guardou.Application.Files;
using Guardou.Domain.Files;
using Guardou.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guardou.IntegrationTests;

public sealed partial class QueryPlanTests : SignedInTests
{
    private readonly QueryPlanRecorder _recorder = new();

    [Fact]
    public async Task ChildrenWithTrashUseTheParentIndex()
    {
        IReadOnlyList<string> plan = await PlanOfAsync(store => store.ChildrenWithTrashAsync(Guid.NewGuid(), CancellationToken));

        Assert.Contains(plan, detail => detail.Contains("USING INDEX IX_Items_ParentId (ParentId=?)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ThePurgeUsesTheTrashIndex()
    {
        IReadOnlyList<string> plan = await PlanOfAsync(store => store.TrashedBeforeAsync(DateTimeOffset.UtcNow, CancellationToken));

        Assert.Contains(plan, detail => detail.Contains("USING INDEX IX_Items_Trashed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RecentItemsDoNotScanTheItemsOfAllOwners()
    {
        IReadOnlyList<string> plan = await PlanOfAsync(store => store.RecentAsync("user", ItemKind.File, 0, 20, CancellationToken));

        Assert.Contains(plan, detail => detail.Contains("USING INDEX IX_Items_CreatedBy_Kind (CreatedBy=? AND Kind=?)", StringComparison.Ordinal));
        Assert.DoesNotContain(plan, detail => TableScan().IsMatch(detail));
    }

    private async Task<IReadOnlyList<string>> PlanOfAsync(Func<IItemStore, Task> query)
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        IItemStore store = scope.ServiceProvider.GetRequiredService<IItemStore>();
        _recorder.Clear();
        await query(store);
        return Assert.Single(_recorder.Plans);
    }

    private protected override TestServerFactory CreateFactory()
    {
        return new TestServerFactory
        {
            TestServices = services => services.ConfigureDbContext<AppDbContext>(options => options.AddInterceptors(_recorder)),
        };
    }

    [GeneratedRegex(@"^SCAN i\d*$")]
    private static partial Regex TableScan();
}
