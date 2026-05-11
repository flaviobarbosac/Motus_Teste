namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class SaleItemResult
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductDescription { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineTotal { get; set; }

    public bool IsCancelled { get; set; }
}
