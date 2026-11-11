using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Guardou.Api.RateLimiting;

internal static class ShareRateLimits
{
    public const string Details = "shareDetails";
    public const string Content = "shareContent";
    public const string Download = "shareDownload";

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static void Add(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = RejectAsync;
            AddPolicy(options, Details, permitsPerMinute: 120);
            AddPolicy(options, Content, permitsPerMinute: 600);
            AddPolicy(options, Download, permitsPerMinute: 30);
        });
    }

    private static void AddPolicy(RateLimiterOptions options, string name, int permitsPerMinute)
    {
        options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitsPerMinute, Window = Window, QueueLimit = 0 }));
    }

    private static async ValueTask RejectAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        HttpContext http = context.HttpContext;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            http.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        await http.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = new ProblemDetails { Status = StatusCodes.Status429TooManyRequests, Detail = "Too many requests." },
        });
    }
}
