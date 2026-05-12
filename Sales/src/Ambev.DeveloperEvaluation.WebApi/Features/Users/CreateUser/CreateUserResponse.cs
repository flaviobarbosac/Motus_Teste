using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;

/// <summary>Utilizador criado (sem palavra-passe).</summary>
public class CreateUserResponse
{
    /// <summary>Identificador do novo utilizador.</summary>
    public Guid Id { get; set; }

    /// <summary>Nome de utilizador.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Telefone.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Perfil.</summary>
    public UserRole Role { get; set; }

    /// <summary>Estado da conta.</summary>
    public UserStatus Status { get; set; }
}
