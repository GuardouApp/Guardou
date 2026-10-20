using System.Security.Claims;

namespace Guardou.Application.Authentication;

public sealed record ExternalLogin(IReadOnlyList<Claim> Claims, string? IdToken);
