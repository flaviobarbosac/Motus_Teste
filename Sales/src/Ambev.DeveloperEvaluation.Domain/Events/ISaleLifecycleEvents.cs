namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Notifies sale lifecycle for integration (log, bus, etc.). No broker required in this prototype.
/// </summary>
public interface ISaleLifecycleEvents
{
    void SaleCreated(Guid saleId, int saleNumber, decimal totalAmount, DateTime occurredAtUtc);

    void SaleModified(Guid saleId, int saleNumber, decimal totalAmount, DateTime occurredAtUtc);

    void SaleCancelled(Guid saleId, int saleNumber, string reason, DateTime occurredAtUtc);

    void ItemCancelled(Guid saleId, Guid productId, string productDescription, int quantity, DateTime occurredAtUtc);
}
