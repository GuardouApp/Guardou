using System.ComponentModel.DataAnnotations;
using Guardou.Api.Authentication;
using Guardou.Application.Files;
using Guardou.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/search")]
public sealed class SearchController(ItemSearch search) : ControllerBase
{
    [HttpGet(Name = "SearchItems")]
    [ProducesResponseType<IReadOnlyList<ItemEntry>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IAsyncEnumerable<ItemEntry>> Search(
        [FromQuery] string? query,
        [FromQuery] string? folderId,
        [FromQuery, Range(1, ItemSearch.MaximumLimit)] int limit = ItemSearch.DefaultLimit,
        CancellationToken cancellationToken = default)
    {
        return search.SearchAsync(User.UserId(), query, folderId, limit, cancellationToken);
    }
}
