using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Errors;

internal sealed class ProblemResponses(IProblemDetailsService problemDetailsService)
{
    public const string CodesExtension = "codes";

    public async Task WriteAsync(HttpContext context, int statusCode, string title, string code)
    {
        context.Response.StatusCode = statusCode;
        ProblemDetails problem = new() { Status = statusCode, Title = title };
        problem.Extensions[CodesExtension] = new[] { code };
        await problemDetailsService.WriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });
    }
}
