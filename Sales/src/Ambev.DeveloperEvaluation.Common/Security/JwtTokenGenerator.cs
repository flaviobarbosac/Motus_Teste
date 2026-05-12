using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.Common.Security;

/// <summary>
/// Emite JWT com <see cref="JsonWebTokenHandler"/> e as mesmas regras que <see cref="JwtBearerPostConfigureOptions"/> (issuer, audience, HS256).
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IOptions<JwtOptions> _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options;

    public string GenerateToken(IUser user)
    {
        var opts = _options.Value;

        var signingKey = JwtTokenParametersFactory.CreateSigningKey(opts.SecretKey);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = opts.Issuer,
            Audience = opts.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow.AddSeconds(-30),
            Expires = DateTime.UtcNow.AddHours(opts.AccessTokenLifetimeHours),
            SigningCredentials = credentials
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
