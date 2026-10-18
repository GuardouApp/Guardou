using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Guardou.Api.Filters;

internal sealed class AntiforgeryValidationFilter(IAntiforgery antiforgery, ProblemDetailsFactory problemDetailsFactory)
    : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (await antiforgery.IsRequestValidAsync(context.HttpContext))
        {
            return;
        }

        ProblemDetails problem = problemDetailsFactory.CreateProblemDetails(
            context.HttpContext,
            StatusCodes.Status400BadRequest,
            title: "The antiforgery token is missing or invalid.");
        context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
    }
}
