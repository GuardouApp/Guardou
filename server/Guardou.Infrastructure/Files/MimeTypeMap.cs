using Guardou.Application.Files;
using Microsoft.AspNetCore.StaticFiles;

namespace Guardou.Infrastructure.Files;

internal sealed class MimeTypeMap : IMimeTypeMap
{
    private const string DefaultMimeType = "application/octet-stream";

    private readonly FileExtensionContentTypeProvider _contentTypes = new();

    public string MimeTypeOf(string name)
    {
        return _contentTypes.TryGetContentType(name, out string? mimeType) ? mimeType : DefaultMimeType;
    }
}
