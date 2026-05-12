namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleResponse
{
    public Guid Id { get; set; }

    public int SaleNumber { get; set; }

    public decimal TotalAmount { get; set; }
}
