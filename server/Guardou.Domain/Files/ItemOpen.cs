namespace Guardou.Domain.Files;

public sealed class ItemOpen
{
    private ItemOpen(Guid itemId, string userId, DateTimeOffset opened)
    {
        ItemId = itemId;
        UserId = userId;
        Opened = opened;
    }

    public Guid ItemId { get; private set; }

    public string UserId { get; private set; }

    public DateTimeOffset Opened { get; private set; }

    public static ItemOpen Create(Guid itemId, string userId, DateTimeOffset now)
    {
        return new ItemOpen(itemId, userId, now);
    }

    public void Reopen(DateTimeOffset now)
    {
        Opened = now;
    }
}
