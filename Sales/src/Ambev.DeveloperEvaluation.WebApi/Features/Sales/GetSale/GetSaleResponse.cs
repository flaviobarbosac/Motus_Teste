namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

public class SaleItemResponse
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

public class GetSaleResponse
{
    public Guid Id { get; set; }

    public int SaleNumber { get; set; }

    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public bool IsCancelled { get; set; }

    public IList<SaleItemResponse> Items { get; set; } = new List<SaleItemResponse>();
}
