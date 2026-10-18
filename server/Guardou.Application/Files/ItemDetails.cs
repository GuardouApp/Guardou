namespace Guardou.Application.Files;

public sealed record ItemDetails(
    ItemEntry Entry,
    IReadOnlyList<Breadcrumb> Ancestors,
    IReadOnlyList<ItemEntry>? Items,
    string? Content);
