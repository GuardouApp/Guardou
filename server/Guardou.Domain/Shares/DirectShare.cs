namespace Guardou.Domain.Shares;

public sealed class DirectShare
{
    private DirectShare(Guid id, Guid itemId, string userId, DateTimeOffset created)
    {
        Id = id;
        ItemId = itemId;
        UserId = userId;
        Created = created;
    }

    public Guid Id { get; private set; }

    public Guid ItemId { get; private set; }

    public string UserId { get; private set; }

    public DateTimeOffset Created { get; private set; }

    public static DirectShare Create(Guid itemId, string userId, DateTimeOffset now)
    {
        return new DirectShare(Guid.CreateVersion7(now), itemId, userId, now);
    }
}
