using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Guardou.IntegrationTests;

internal sealed class FakeIdentityProvider : HttpMessageHandler
{
    public const string Issuer = "https://id.example.com";
    public const string ClientId = "guardou";
    public const string EndSessionEndpoint = Issuer + "/logout";
    public const string PictureUrl = Issuer + "/picture.png";

    private const string KeyId = "test-key";

    private readonly RSA _rsa = RSA.Create(2048);

    public string? Nonce { get; set; }

    public Dictionary<string, object> Claims { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsoluteUri == PictureUrl)
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(PreviewsApi.Png(300, 200)) };
        }

        object? body = request.RequestUri?.AbsolutePath switch
        {
            "/.well-known/openid-configuration" => Metadata(),
            "/jwks" => Keys(),
            "/token" => Tokens(),
            "/userinfo" => Claims,
            _ => null,
        };
        await Task.CompletedTask;
        return body is null
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(body) };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _rsa.Dispose();
        }

        base.Dispose(disposing);
    }

    private static Dictionary<string, object> Metadata()
    {
        return new Dictionary<string, object>
        {
            ["issuer"] = Issuer,
            ["authorization_endpoint"] = Issuer + "/authorize",
            ["token_endpoint"] = Issuer + "/token",
            ["userinfo_endpoint"] = Issuer + "/userinfo",
            ["jwks_uri"] = Issuer + "/jwks",
            ["end_session_endpoint"] = EndSessionEndpoint,
            ["response_types_supported"] = new[] { "code" },
            ["subject_types_supported"] = new[] { "public" },
            ["id_token_signing_alg_values_supported"] = new[] { SecurityAlgorithms.RsaSha256 },
        };
    }

    private Dictionary<string, object> Keys()
    {
        RSAParameters parameters = _rsa.ExportParameters(includePrivateParameters: false);
        return new Dictionary<string, object>
        {
            ["keys"] = new[]
            {
                new Dictionary<string, string>
                {
                    ["kty"] = "RSA",
                    ["use"] = "sig",
                    ["alg"] = SecurityAlgorithms.RsaSha256,
                    ["kid"] = KeyId,
                    ["n"] = Base64UrlEncoder.Encode(parameters.Modulus),
                    ["e"] = Base64UrlEncoder.Encode(parameters.Exponent),
                },
            },
        };
    }

    private Dictionary<string, object> Tokens()
    {
        Dictionary<string, object> claims = new(Claims) { ["nonce"] = Nonce ?? string.Empty };
        string idToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = ClientId,
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(_rsa) { KeyId = KeyId }, SecurityAlgorithms.RsaSha256),
        });
        return new Dictionary<string, object>
        {
            ["id_token"] = idToken,
            ["access_token"] = "access-token",
            ["token_type"] = "Bearer",
            ["expires_in"] = 300,
        };
    }
}
