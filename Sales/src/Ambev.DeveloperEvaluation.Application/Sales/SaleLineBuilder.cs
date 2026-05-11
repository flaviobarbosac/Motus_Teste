using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Application.Sales;

internal static class SaleLineBuilder
{
    public static (List<SaleItem> Items, decimal TotalAmount) BuildLines(
        IEnumerable<CreateSaleItemCommand> lines,
        Guid saleId,
        ISaleLineDiscountCalculator calculator)
    {
        var list = new List<SaleItem>();
        decimal total = 0;

        foreach (var line in lines)
        {
            var pricing = calculator.Calculate(line.Quantity, line.UnitPrice);
            var item = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = saleId,
                ProductId = line.ProductId,
                ProductDescription = line.ProductDescription,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountAmount = pricing.DiscountAmount,
                LineTotal = pricing.LineTotal,
                IsCancelled = line.IsCancelled
            };

            if (!line.IsCancelled)
                total += pricing.LineTotal;

            list.Add(item);
        }

        return (list, decimal.Round(total, 2, MidpointRounding.AwayFromZero));
    }
}
