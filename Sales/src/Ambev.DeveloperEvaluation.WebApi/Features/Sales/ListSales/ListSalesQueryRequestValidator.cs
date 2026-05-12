using Ambev.DeveloperEvaluation.Domain;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesQueryRequest
{
    public int Page { get; set; } = 1;

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
