using System.Globalization;
using System.Threading.RateLimiting;
using Guardou.Api.Errors;
using Guardou.Application.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Guardou.Api.Authentication;

internal static class AuthenticationRateLimit
{
    public const string Policy = "authentication";
    public const string Code = "rateLimited";

    private const string UnknownClient = "unknown";
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static void Add(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(Policy, PartitionOf);
            options.OnRejected = RejectAsync;
        });
    }

    private static RateLimitPartition<string> PartitionOf(HttpContext context)
    {
        int limit = context.RequestServices.GetRequiredService<IOptions<AuthenticationLimits>>().Value.RateLimit;
        string client = context.Connection.RemoteIpAddress?.ToString() ?? UnknownClient;
        return RateLimitPartition.GetFixedWindowLimiter(client, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = Window,
            QueueLimit = 0,
        });
    }

    private static async ValueTask RejectAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        await context.HttpContext.RequestServices.GetRequiredService<ProblemResponses>().WriteAsync(
            context.HttpContext,
            StatusCodes.Status429TooManyRequests,
            "Too many requests. Try again later.",
            Code);
    }
}
