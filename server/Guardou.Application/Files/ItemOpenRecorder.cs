using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed class ItemOpenRecorder
{
    private readonly IItemOpenStore _opens;
    private readonly AccessibleItems _items;
    private readonly TimeProvider _time;

    internal ItemOpenRecorder(IItemOpenStore opens, AccessibleItems items, TimeProvider time)
    {
        _opens = opens;
        _items = items;
        _time = time;
    }

    public async Task RecordAsync(string userId, string id, CancellationToken cancellationToken)
    {
        Item item = (await _items.FindActiveAsync(userId, id, cancellationToken)).Item;
        if (item.IsFolder)
        {
            throw new InvalidItemRequestException("A folder has no content.");
        }

        DateTimeOffset now = _time.GetUtcNow();
        if (await _opens.FindAsync(item.Id, userId, cancellationToken) is { } open)
        {
            open.Reopen(now);
        }
        else
        {
            _opens.Add(ItemOpen.Create(item.Id, userId, now));
        }

        await _opens.SaveAsync(cancellationToken);
    }
}
