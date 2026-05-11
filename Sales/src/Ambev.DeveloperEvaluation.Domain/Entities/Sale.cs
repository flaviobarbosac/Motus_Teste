using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Sale aggregate root. Customer and branch are external identities with denormalized descriptions.
/// </summary>
public class Sale : BaseEntity
{
    public int SaleNumber { get; set; }

    public DateTime SaleDate { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public bool IsCancelled { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
