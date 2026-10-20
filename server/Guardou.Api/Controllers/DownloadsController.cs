using System.Net.Mime;
using Guardou.Api.Authentication;
using Guardou.Api.ActionResults;
using Guardou.Application.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class DownloadsController(ItemDownloads downloads) : ControllerBase
{
    private const string SandboxPolicy = "sandbox";
    private const string NoSniff = "nosniff";

    [HttpGet("{id}/download", Name = "DownloadItem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string id, CancellationToken cancellationToken)
    {
        FileDownload? file = await downloads.TryOpenFileAsync(User.UserId(), id, cancellationToken);
        if (file is null)
        {
            return await ArchiveAsync([id], cancellationToken);
        }

        return File(file.Content, file.MimeType, file.Name, file.Modified, EntityTagOf(file), enableRangeProcessing: true);
    }

    [HttpGet("{id}/view", Name = "ViewItem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        FileDownload file = await downloads.OpenFileAsync(User.UserId(), id, cancellationToken);
        ContentDispositionHeaderValue disposition = new(DispositionTypeNames.Inline);
        disposition.SetHttpFileName(file.Name);
        Response.Headers.ContentDisposition = disposition.ToString();
        if (file.MimeType != MediaTypeNames.Application.Pdf)
        {
            Response.Headers.ContentSecurityPolicy = SandboxPolicy;
        }

        Response.Headers.XContentTypeOptions = NoSniff;
        return File(file.Content, file.MimeType, file.Modified, EntityTagOf(file), enableRangeProcessing: true);
    }

    [HttpGet("archive", Name = "DownloadArchive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Archive([FromQuery] IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        return ArchiveAsync(ids, cancellationToken);
    }

    private static EntityTagHeaderValue EntityTagOf(FileDownload file)
    {
        return new EntityTagHeaderValue($"\"{file.Modified.UtcTicks:x}\"");
    }

    private async Task<IActionResult> ArchiveAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        ItemArchive archive = await downloads.PrepareArchiveAsync(User.UserId(), ids, cancellationToken);
        return new ArchiveResult(archive, downloads);
    }
}
