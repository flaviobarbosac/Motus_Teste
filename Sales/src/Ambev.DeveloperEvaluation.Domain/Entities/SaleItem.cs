namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Line item of a sale. Product identity is external; description is denormalized for reads.
/// </summary>
public class SaleItem
{
    public Guid Id { get; set; }

    public Guid SaleId { get; set; }

    public Guid ProductId { get; set; }

    public string ProductDescription { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineTotal { get; set; }

    public bool IsCancelled { get; set; }
}
