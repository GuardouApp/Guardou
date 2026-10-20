using Microsoft.AspNetCore.Http;

namespace Guardou.Infrastructure.Web;

public static class HttpRequestExtensions
{
    public static string Origin(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }
}
