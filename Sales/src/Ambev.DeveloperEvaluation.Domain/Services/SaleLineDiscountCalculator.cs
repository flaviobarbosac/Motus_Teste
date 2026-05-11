using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <inheritdoc />
/// <remarks>
/// Rules: below 4 units — no discount; 4–9 — 10%; 10–20 — 20%; above 20 — not allowed.
/// </remarks>
public sealed class SaleLineDiscountCalculator : ISaleLineDiscountCalculator
{
    public int MaxQuantityPerProduct => 20;

    public SaleLinePricing Calculate(int quantity, decimal unitPrice)
    {
        if (quantity < 1)
            throw new DomainException("Quantity must be at least 1.");

        if (quantity > MaxQuantityPerProduct)
            throw new DomainException($"Cannot sell more than {MaxQuantityPerProduct} identical items per product.");

        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");

        var subtotal = decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);
        var rate = GetDiscountRate(quantity);
        var discountAmount = decimal.Round(subtotal * rate, 2, MidpointRounding.AwayFromZero);
        var lineTotal = decimal.Round(subtotal - discountAmount, 2, MidpointRounding.AwayFromZero);

        return new SaleLinePricing(subtotal, discountAmount, lineTotal);
    }

    private static decimal GetDiscountRate(int quantity)
    {
        if (quantity < 4)
            return 0m;

        if (quantity < 10)
            return 0.10m;

        return 0.20m;
    }
}
