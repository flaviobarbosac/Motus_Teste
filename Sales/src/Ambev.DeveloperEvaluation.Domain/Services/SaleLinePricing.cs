namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// Monetary breakdown for a single sale line after quantity-tier discount.
/// </summary>
public readonly record struct SaleLinePricing(decimal Subtotal, decimal DiscountAmount, decimal LineTotal);
