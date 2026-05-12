using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public sealed class SaleLifecycleLogger : ISaleLifecycleEvents
{
    private readonly ILogger<SaleLifecycleLogger> _logger;

    public SaleLifecycleLogger(ILogger<SaleLifecycleLogger> logger)
    {
        _logger = logger;
    }

    public void SaleCreated(Guid saleId, int saleNumber, decimal totalAmount, DateTime occurredAtUtc)
    {
        _logger.LogInformation(
            "SaleCreated: SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount} OccurredAtUtc={OccurredAtUtc}",
            saleId, saleNumber, totalAmount, occurredAtUtc);
    }

    public void SaleModified(Guid saleId, int saleNumber, decimal totalAmount, DateTime occurredAtUtc)
    {
        _logger.LogInformation(
            "SaleModified: SaleId={SaleId} SaleNumber={SaleNumber} TotalAmount={TotalAmount} OccurredAtUtc={OccurredAtUtc}",
            saleId, saleNumber, totalAmount, occurredAtUtc);
    }

    public void SaleCancelled(Guid saleId, int saleNumber, string reason, DateTime occurredAtUtc)
    {
        _logger.LogInformation(
            "SaleCancelled: SaleId={SaleId} SaleNumber={SaleNumber} Reason={Reason} OccurredAtUtc={OccurredAtUtc}",
            saleId, saleNumber, reason, occurredAtUtc);
    }

    public void ItemCancelled(Guid saleId, Guid productId, string productDescription, int quantity, DateTime occurredAtUtc)
    {
        _logger.LogInformation(
            "ItemCancelled: SaleId={SaleId} ProductId={ProductId} ProductDescription={ProductDescription} Quantity={Quantity} OccurredAtUtc={OccurredAtUtc}",
            saleId, productId, productDescription, quantity, occurredAtUtc);
    }
}
