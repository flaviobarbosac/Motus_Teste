namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequest
{
    public int SaleNumber { get; set; }

    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public bool IsCancelled { get; set; }

    public IList<CreateSaleItemRequest> Items { get; set; } = new List<CreateSaleItemRequest>();
}
