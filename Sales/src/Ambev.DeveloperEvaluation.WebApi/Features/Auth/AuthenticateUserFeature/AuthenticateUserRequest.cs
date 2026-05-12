namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// Pedido de autenticação (login com email e palavra-passe).
/// </summary>
public class AuthenticateUserRequest
{
    /// <summary>Email do utilizador (conta ativa).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Palavra-passe em texto plano (transporte deve usar HTTPS em produção).</summary>
    public string Password { get; set; } = string.Empty;
}
