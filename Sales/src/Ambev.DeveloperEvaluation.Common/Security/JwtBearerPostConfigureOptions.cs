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

        // O handler nativo só remove um "Bearer "; o Swagger UI já prefixa "Bearer ".
        // Se o utilizador colar "Bearer eyJ..." no Authorize, o cabeçalho fica "Bearer Bearer eyJ..." → invalid_token.
        options.Events ??= new JwtBearerEvents();
        var priorMessageReceived = options.Events.OnMessageReceived;
        options.Events.OnMessageReceived = async context =>
        {
            if (priorMessageReceived is not null)
                await priorMessageReceived(context).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(context.Token))
            {
                var fromToken = JwtBearerAuthorizationHeaderNormalizer.ExtractJwt(context.Token);
                if (!string.IsNullOrWhiteSpace(fromToken))
                    context.Token = fromToken;
                return;
            }

            var extracted = JwtBearerAuthorizationHeaderNormalizer.ExtractJwt(
                context.Request.Headers.Authorization.ToString());
            if (!string.IsNullOrWhiteSpace(extracted))
                context.Token = extracted;
        };
    }
}
