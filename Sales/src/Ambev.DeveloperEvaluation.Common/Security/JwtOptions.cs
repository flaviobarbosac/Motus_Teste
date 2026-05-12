namespace Ambev.DeveloperEvaluation.Common.Security;

/// <summary>
/// Configuração JWT partilhada entre emissão (<see cref="JwtTokenGenerator"/>) e validação (JwtBearer).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Chave simétrica (UTF-8). Mínimo recomendado: 32 caracteres.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Emissor do token (claim <c>iss</c>).</summary>
    public string Issuer { get; set; } = "Ambev.DeveloperEvaluation";

    /// <summary>Público do token (claim <c>aud</c>).</summary>
    public string Audience { get; set; } = "Ambev.DeveloperEvaluation.Api";

    /// <summary>Duração do access token em horas.</summary>
    public int AccessTokenLifetimeHours { get; set; } = 8;
}
