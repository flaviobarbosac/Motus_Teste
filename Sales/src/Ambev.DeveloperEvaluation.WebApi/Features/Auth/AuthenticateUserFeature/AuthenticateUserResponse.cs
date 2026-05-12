namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;

/// <summary>
/// Dados devolvidos após login bem-sucedido; inclui o JWT em <see cref="Token"/>.
/// </summary>
public sealed class AuthenticateUserResponse
{
    /// <summary>JWT para o cabeçalho <c>Authorization: Bearer</c>.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Email do utilizador autenticado.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Nome de utilizador.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Perfil (texto), alinhado com a enumeração de domínio.</summary>
    public string Role { get; set; } = string.Empty;
}
