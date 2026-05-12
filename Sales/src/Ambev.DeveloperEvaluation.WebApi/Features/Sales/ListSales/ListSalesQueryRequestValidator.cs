using Ambev.DeveloperEvaluation.Domain;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>Parâmetros de query para listar vendas com paginação.</summary>
public class ListSalesQueryRequest
{
    /// <summary>Número da página (≥ 1). Omisso: 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Tamanho da página (limites mínimo e máximo definidos na API). Omisso: valor por omissão do sistema.</summary>
    public int PageSize { get; set; } = SalesListPagination.DefaultPageSize;
}

public class ListSalesQueryRequestValidator : AbstractValidator<ListSalesQueryRequest>
{
    public ListSalesQueryRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, SalesListPagination.MaxPageSize);
    }
}
