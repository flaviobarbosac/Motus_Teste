using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

/// <summary>
/// Pedido para criar utilizador (registo). Campos sujeitos a validação (email, telefone, complexidade da palavra-passe, etc.).
/// </summary>
public class CreateUserRequest
{
    /// <summary>Nome de utilizador único.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Palavra-passe (requisitos mínimos definidos na API).</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Telefone no formato esperado pela validação.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Email único.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Estado inicial da conta.</summary>
    public UserStatus Status { get; set; }

    /// <summary>Perfil (enumeração de domínio).</summary>
    public UserRole Role { get; set; }
}