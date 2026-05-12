using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.DeleteSale;

public class DeleteSaleRequest
{
    public Guid Id { get; set; }
}

public class DeleteSaleRequestValidator : AbstractValidator<DeleteSaleRequest>
{
    public DeleteSaleRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
