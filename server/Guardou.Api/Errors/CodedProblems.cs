using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Errors;

internal static class CodedProblems
{
    public static ObjectResult CodedProblem(this ControllerBase controller, int statusCode, string title, string code)
    {
        ProblemDetails problem = controller.ProblemDetailsFactory.CreateProblemDetails(controller.HttpContext, statusCode, title);
        problem.Extensions[ProblemResponses.CodesExtension] = new[] { code };
        return new ObjectResult(problem) { StatusCode = statusCode };
    }
}
