using System.Net.Mime;
using Guardou.Application.Files;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Guardou.Api.ActionResults;

internal static class FileResponses
{
    private const string SandboxPolicy = "sandbox";
    private const string NoSniff = "nosniff";
    private const string WebpMimeType = "image/webp";
    private const string PreviewCachePolicy = "private, max-age=31536000, immutable";
    private const string PreviewRetrySeconds = "2";
    private const string PictureCachePolicy = "private, no-cache";

    public static IActionResult Download(this ControllerBase controller, FileDownload file)
    {
        return controller.File(file.Content, file.MimeType, file.Name, file.Modified, EntityTagOf(file), enableRangeProcessing: true);
    }

    public static IActionResult View(this ControllerBase controller, FileDownload file)
    {
        IHeaderDictionary headers = controller.Response.Headers;
        ContentDispositionHeaderValue disposition = new(DispositionTypeNames.Inline);
        disposition.SetHttpFileName(file.Name);
        headers.ContentDisposition = disposition.ToString();
        if (file.MimeType != MediaTypeNames.Application.Pdf)
        {
            headers.ContentSecurityPolicy = SandboxPolicy;
        }

        headers.XContentTypeOptions = NoSniff;
        return controller.File(file.Content, file.MimeType, file.Modified, EntityTagOf(file), enableRangeProcessing: true);
    }

    public static IActionResult Preview(this ControllerBase controller, Stream? preview)
    {
        if (preview is null)
        {
            controller.Response.Headers.RetryAfter = PreviewRetrySeconds;
            return controller.Accepted();
        }

        controller.Response.Headers.CacheControl = PreviewCachePolicy;
        return controller.File(preview, WebpMimeType);
    }

    public static IActionResult Picture(this ControllerBase controller, ProfilePicture? picture)
    {
        if (picture is null)
        {
            return controller.Problem(statusCode: StatusCodes.Status404NotFound, title: "The user has no picture.");
        }

        controller.Response.Headers.CacheControl = PictureCachePolicy;
        EntityTagHeaderValue entityTag = new($"\"{picture.Modified.UtcTicks}\"");
        return controller.File(picture.Content, WebpMimeType, picture.Modified, entityTag);
    }

    private static EntityTagHeaderValue EntityTagOf(FileDownload file)
    {
        return new EntityTagHeaderValue($"\"{file.Modified.UtcTicks:x}\"");
    }
}
