using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ambev.DeveloperEvaluation.Common.Security;

public static class AuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(o =>
            {
                o.SecretKey = o.SecretKey.Trim();
                o.Issuer = o.Issuer.Trim();
                o.Audience = o.Audience.Trim();
            })
            .Validate(
                o => o.SecretKey.Length >= JwtTokenParametersFactory.MinimumSecretKeyLength,
                $"Jwt:SecretKey must be at least {JwtTokenParametersFactory.MinimumSecretKeyLength} characters.")
            .Validate(o => !string.IsNullOrEmpty(o.Issuer), "Jwt:Issuer is required.")
            .Validate(o => !string.IsNullOrEmpty(o.Audience), "Jwt:Audience is required.")
            .Validate(o => o.AccessTokenLifetimeHours is > 0 and <= 168, "Jwt:AccessTokenLifetimeHours must be between 1 and 168.")
            .ValidateOnStart();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>();

        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { });

        services.AddAuthorization();

        return services;
    }
}
