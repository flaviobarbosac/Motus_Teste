using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Ambev.DeveloperEvaluation.Common.Security;

/// <summary>
/// Aplica os mesmos <see cref="TokenValidationParameters"/> derivados de <see cref="JwtOptions"/> ao middleware Bearer.
/// </summary>
internal sealed class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IOptionsMonitor<JwtOptions> _jwt;

    public JwtBearerPostConfigureOptions(IOptionsMonitor<JwtOptions> jwt) => _jwt = jwt;

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        var jwt = _jwt.CurrentValue;

        var signingKey = JwtTokenParametersFactory.CreateSigningKey(jwt.SecretKey);
        options.TokenValidationParameters = JwtTokenParametersFactory.CreateValidationParameters(jwt, signingKey);
        options.UseSecurityTokenValidators = false;
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.MapInboundClaims = true;
    }
}
