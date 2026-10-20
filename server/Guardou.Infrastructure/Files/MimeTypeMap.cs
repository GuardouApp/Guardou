using System.Collections.Frozen;
using Guardou.Application.Files;
using Microsoft.AspNetCore.StaticFiles;

namespace Guardou.Infrastructure.Files;

internal sealed class MimeTypeMap : IMimeTypeMap
{
    private const string DefaultMimeType = "application/octet-stream";

    private static readonly FrozenDictionary<string, string> PhotoMimeTypes = new Dictionary<string, string>
    {
        [".heic"] = "image/heic",
        [".heif"] = "image/heif",
        [".3fr"] = "image/x-hasselblad-3fr",
        [".arw"] = "image/x-sony-arw",
        [".cr2"] = "image/x-canon-cr2",
        [".cr3"] = "image/x-canon-cr3",
        [".dng"] = "image/x-adobe-dng",
        [".erf"] = "image/x-epson-erf",
        [".kdc"] = "image/x-kodak-kdc",
        [".mrw"] = "image/x-minolta-mrw",
        [".nef"] = "image/x-nikon-nef",
        [".nrw"] = "image/x-nikon-nrw",
        [".orf"] = "image/x-olympus-orf",
        [".pef"] = "image/x-pentax-pef",
        [".raf"] = "image/x-fuji-raf",
        [".rw2"] = "image/x-panasonic-rw2",
        [".sr2"] = "image/x-sony-sr2",
        [".srf"] = "image/x-sony-srf",
        [".srw"] = "image/x-samsung-srw",
        [".x3f"] = "image/x-sigma-x3f",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private readonly FileExtensionContentTypeProvider _contentTypes = CreateContentTypes();

    public string MimeTypeOf(string name)
    {
        return _contentTypes.TryGetContentType(name, out string? mimeType) ? mimeType : DefaultMimeType;
    }

    private static FileExtensionContentTypeProvider CreateContentTypes()
    {
        FileExtensionContentTypeProvider contentTypes = new();
        foreach ((string extension, string mimeType) in PhotoMimeTypes)
        {
            contentTypes.Mappings[extension] = mimeType;
        }

        return contentTypes;
    }
}
