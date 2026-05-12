using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ambev.DeveloperEvaluation.Common.Security;

/// <summary>
/// Parâmetros de assinatura e validação alinhados (mesma chave, issuer, audience).
/// </summary>
public static class JwtTokenParametersFactory
{
    public const int MinimumSecretKeyLength = 32;

    public static SymmetricSecurityKey CreateSigningKey(string secretKeyUtf8)
    {
        var bytes = Encoding.UTF8.GetBytes(secretKeyUtf8);
        return new SymmetricSecurityKey(bytes);
    }

    /// <summary>
    /// Parâmetros usados pelo JwtBearer com JsonWebTokenHandler (.NET 8).
    /// </summary>
    public static TokenValidationParameters CreateValidationParameters(JwtOptions options, SymmetricSecurityKey signingKey) =>
        new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = options.Issuer.Trim(),
            ValidateAudience = true,
            ValidAudience = options.Audience.Trim(),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
}
