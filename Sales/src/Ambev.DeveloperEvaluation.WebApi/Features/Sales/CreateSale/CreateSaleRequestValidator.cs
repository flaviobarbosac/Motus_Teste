using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.SaleNumber).GreaterThan(0);
        RuleFor(x => x.SaleDate).NotEqual(default(DateTime));
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductDescription).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
