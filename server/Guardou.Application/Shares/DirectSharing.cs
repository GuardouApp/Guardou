using Guardou.Application.Files;
using Guardou.Application.Users;
using Guardou.Domain.Files;
using Guardou.Domain.Shares;

namespace Guardou.Application.Shares;

public sealed class DirectSharing
{
    private readonly IDirectShareStore _shares;
    private readonly IItemStore _store;
    private readonly OwnedItems _items;
    private readonly IUserDirectory _users;
    private readonly ItemEntryFactory _entries;
    private readonly TimeProvider _time;

    internal DirectSharing(
        IDirectShareStore shares,
        IItemStore store,
        OwnedItems items,
        IUserDirectory users,
        ItemEntryFactory entries,
        TimeProvider time)
    {
        _shares = shares;
        _store = store;
        _items = items;
        _users = users;
        _entries = entries;
        _time = time;
    }

    public async Task<IReadOnlyList<UserSummary>> ListRecipientsAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        Item item = await _items.FindShareableAsync(ownerId, id, cancellationToken);
        return await RecipientsAsync(item, cancellationToken);
    }

    public async Task<IReadOnlyList<UserSummary>> AddRecipientAsync(
        string ownerId,
        string id,
        string userId,
        CancellationToken cancellationToken)
    {
        Item item = await _items.FindShareableAsync(ownerId, id, cancellationToken);
        if (userId == ownerId)
        {
            throw new InvalidItemRequestException("You cannot share an item with yourself.");
        }

        if ((await _users.FindAsync([userId], cancellationToken)).Count == 0)
        {
            throw new UserNotFoundException(userId);
        }

        if (await _shares.FindAsync(item.Id, userId, cancellationToken) is null)
        {
            _shares.Add(DirectShare.Create(item.Id, userId, _time.GetUtcNow()));
            await _shares.SaveAsync(cancellationToken);
        }

        return await RecipientsAsync(item, cancellationToken);
    }

    public async Task RemoveRecipientAsync(string ownerId, string id, string userId, CancellationToken cancellationToken)
    {
        Item item = await _items.FindShareableAsync(ownerId, id, cancellationToken);
        if (await _shares.FindAsync(item.Id, userId, cancellationToken) is { } share)
        {
            _shares.Remove(share);
            await _shares.SaveAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ItemEntry>> SharedWithAsync(string userId, CancellationToken cancellationToken)
    {
        List<Item> items = [];
        foreach (DirectShare share in await _shares.OfUserAsync(userId, cancellationToken))
        {
            if (await _store.FindAnyAsync(share.ItemId, cancellationToken) is { } item
                && await _items.TryFindActiveAsync(item.OwnerId, item.Id, cancellationToken) is not null)
            {
                items.Add(item);
            }
        }

        return await _entries.CreateSharedRootsAsync(items, cancellationToken);
    }

    private async Task<IReadOnlyList<UserSummary>> RecipientsAsync(Item item, CancellationToken cancellationToken)
    {
        IReadOnlyList<DirectShare> shares = await _shares.OfItemAsync(item.Id, cancellationToken);
        IReadOnlyList<UserSummary> users = await _users.FindAsync(shares.Select(share => share.UserId).ToList(), cancellationToken);
        return users.OrderBy(user => user.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
    }
}
