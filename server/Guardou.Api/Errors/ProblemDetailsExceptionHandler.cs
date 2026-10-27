using Guardou.Application.Files;
using Guardou.Application.Search;
using Guardou.Application.Settings;
using Guardou.Application.Shares;
using Guardou.Application.Tasks;
using Guardou.Application.Users;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Guardou.Api.Errors;

internal sealed partial class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    private const string CodesExtension = "codes";

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode = StatusCodeOf(exception);
        bool isServerError = statusCode >= StatusCodes.Status500InternalServerError;
        if (isServerError)
        {
            LogUnhandledException(exception, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = ProblemOf(exception, statusCode, isServerError),
        });
    }

    private static ProblemDetails ProblemOf(Exception exception, int statusCode, bool isServerError)
    {
        ProblemDetails problem = new() { Status = statusCode, Detail = isServerError ? null : exception.Message };
        if (exception is UserAccountRejectedException rejected)
        {
            problem.Extensions[CodesExtension] = rejected.Errors.Select(error => error.Code).ToList();
        }

        return problem;
    }

    private static int StatusCodeOf(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException badRequest => badRequest.StatusCode,
            InvalidItemRequestException => StatusCodes.Status400BadRequest,
            InvalidSearchQueryException => StatusCodes.Status400BadRequest,
            ItemNotFoundException => StatusCodes.Status404NotFound,
            ItemConflictException => StatusCodes.Status409Conflict,
            ShareNotFoundException => StatusCodes.Status404NotFound,
            UserAccountRejectedException => StatusCodes.Status400BadRequest,
            UserNotFoundException => StatusCodes.Status404NotFound,
            InvalidProfilePictureException => StatusCodes.Status400BadRequest,
            TaskNotFoundException => StatusCodes.Status404NotFound,
            TaskConflictException => StatusCodes.Status409Conflict,
            InvalidSettingsException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The request {Method} {Path} failed.")]
    private partial void LogUnhandledException(Exception exception, string method, PathString path);
}
