using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Calculates per-line discount from identical-item quantity tiers and line totals.
/// </summary>
public interface ISaleLineDiscountCalculator
{
    /// <summary>
    /// Maximum units of the same product allowed on a single line.
    /// </summary>
    int MaxQuantityPerProduct { get; }

    /// <summary>
    /// Computes subtotal, discount and line total for one product line.
    /// </summary>
    /// <exception cref="DomainException">Invalid quantity or unit price.</exception>
    SaleLinePricing Calculate(int quantity, decimal unitPrice);
}
