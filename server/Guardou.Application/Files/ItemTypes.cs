using System.Collections.Frozen;
using Guardou.Domain.Files;

namespace Guardou.Application.Files;

public static class ItemTypes
{
    private static readonly FrozenDictionary<string, ItemType> ExactTypes = new Dictionary<string, ItemType>
    {
        ["application/pdf"] = ItemType.Pdf,
        ["application/msword"] = ItemType.Document,
        ["application/rtf"] = ItemType.Document,
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = ItemType.Document,
        ["application/vnd.oasis.opendocument.text"] = ItemType.Document,
        ["application/vnd.ms-excel"] = ItemType.Spreadsheet,
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = ItemType.Spreadsheet,
        ["application/vnd.oasis.opendocument.spreadsheet"] = ItemType.Spreadsheet,
        ["text/csv"] = ItemType.Spreadsheet,
        ["application/vnd.ms-powerpoint"] = ItemType.Presentation,
        ["application/vnd.openxmlformats-officedocument.presentationml.presentation"] = ItemType.Presentation,
        ["application/vnd.oasis.opendocument.presentation"] = ItemType.Presentation,
        ["application/zip"] = ItemType.Archive,
        ["application/x-zip-compressed"] = ItemType.Archive,
        ["application/x-7z-compressed"] = ItemType.Archive,
        ["application/x-rar-compressed"] = ItemType.Archive,
        ["application/vnd.rar"] = ItemType.Archive,
        ["application/x-tar"] = ItemType.Archive,
        ["application/gzip"] = ItemType.Archive,
        ["application/x-gzip"] = ItemType.Archive,
        ["application/x-bzip2"] = ItemType.Archive,
        ["application/x-xz"] = ItemType.Archive,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly (string Prefix, ItemType Type)[] PrefixTypes =
    [
        ("image/", ItemType.Image),
        ("video/", ItemType.Video),
        ("audio/", ItemType.Audio),
        ("text/", ItemType.Text),
    ];

    public static ItemType Of(Item item, IMimeTypeMap mimeTypes)
    {
        return item.IsFolder ? ItemType.Folder : OfMimeType(mimeTypes.MimeTypeOf(item.Name));
    }

    private static ItemType OfMimeType(string mimeType)
    {
        if (ExactTypes.TryGetValue(mimeType, out ItemType type))
        {
            return type;
        }

        foreach ((string prefix, ItemType prefixType) in PrefixTypes)
        {
            if (mimeType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return prefixType;
            }
        }

        return ItemType.Other;
    }
}
