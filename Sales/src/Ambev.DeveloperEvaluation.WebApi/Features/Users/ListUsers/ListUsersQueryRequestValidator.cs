using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUsers;

/// <summary>Parâmetros de query para listar utilizadores com paginação.</summary>
public class ListUsersQueryRequest
{
    /// <summary>Número da página (≥ 1). Omisso: 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Tamanho da página (1–100). Omisso: 20.</summary>
    public int PageSize { get; set; } = 20;
}

public class ListUsersQueryRequestValidator : AbstractValidator<ListUsersQueryRequest>
{
    public ListUsersQueryRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
