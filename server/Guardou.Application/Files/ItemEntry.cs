using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public sealed record ItemEntry(
    string Id,
    string? ParentId,
    string Name,
    ItemKind Kind,
    long Size,
    string? MimeType,
    DateTimeOffset Modified,
    DateTimeOffset Created,
    DateTimeOffset? Opened,
    ItemCreator Creator,
    bool HasPreview,
    ItemPermissions Permissions);
