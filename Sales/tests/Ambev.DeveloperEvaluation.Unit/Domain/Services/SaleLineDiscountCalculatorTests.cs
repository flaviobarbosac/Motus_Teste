using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using Xunit;

public class SaleLineDiscountCalculatorTests
{
    private readonly SaleLineDiscountCalculator _sut = new();

    [Theory(DisplayName = "Quantities 1 to 3 have no discount")]
    [InlineData(1, 100, 100, 0, 100)]
    [InlineData(2, 50, 100, 0, 100)]
    [InlineData(3, 10, 30, 0, 30)]
    public void Given_QuantityBelowFour_When_Calculate_Then_NoDiscount(
        int quantity, decimal unitPrice, decimal expectedSubtotal, decimal expectedDiscount, decimal expectedLineTotal)
    {
        var result = _sut.Calculate(quantity, unitPrice);

        Assert.Equal(expectedSubtotal, result.Subtotal);
        Assert.Equal(expectedDiscount, result.DiscountAmount);
        Assert.Equal(expectedLineTotal, result.LineTotal);
    }

    [Theory(DisplayName = "Quantities 4 to 9 have 10% discount on line subtotal")]
    [InlineData(4, 100, 400, 40, 360)]
    [InlineData(5, 10, 50, 5, 45)]
    [InlineData(9, 100, 900, 90, 810)]
    public void Given_QuantityBetweenFourAndNine_When_Calculate_Then_TenPercentDiscount(
        int quantity, decimal unitPrice, decimal expectedSubtotal, decimal expectedDiscount, decimal expectedLineTotal)
    {
        var result = _sut.Calculate(quantity, unitPrice);

        Assert.Equal(expectedSubtotal, result.Subtotal);
        Assert.Equal(expectedDiscount, result.DiscountAmount);
        Assert.Equal(expectedLineTotal, result.LineTotal);
    }

    [Theory(DisplayName = "Quantities 10 to 20 have 20% discount on line subtotal")]
    [InlineData(10, 10, 100, 20, 80)]
    [InlineData(15, 2, 30, 6, 24)]
    [InlineData(20, 100, 2000, 400, 1600)]
    public void Given_QuantityBetweenTenAndTwenty_When_Calculate_Then_TwentyPercentDiscount(
        int quantity, decimal unitPrice, decimal expectedSubtotal, decimal expectedDiscount, decimal expectedLineTotal)
    {
        var result = _sut.Calculate(quantity, unitPrice);

        Assert.Equal(expectedSubtotal, result.Subtotal);
        Assert.Equal(expectedDiscount, result.DiscountAmount);
        Assert.Equal(expectedLineTotal, result.LineTotal);
    }

    [Fact(DisplayName = "Quantity above 20 throws domain exception")]
    public void Given_QuantityAboveMax_When_Calculate_Then_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => _sut.Calculate(21, 10m));

        Assert.Contains("20", ex.Message);
    }

    [Fact(DisplayName = "Quantity zero throws domain exception")]
    public void Given_QuantityZero_When_Calculate_Then_Throws()
    {
        Assert.Throws<DomainException>(() => _sut.Calculate(0, 10m));
    }

    [Fact(DisplayName = "Negative unit price throws domain exception")]
    public void Given_NegativeUnitPrice_When_Calculate_Then_Throws()
    {
        Assert.Throws<DomainException>(() => _sut.Calculate(1, -1m));
    }

    [Fact(DisplayName = "Max quantity constant is 20")]
    public void Given_Calculator_When_MaxQuantity_Then_Twenty()
    {
        Assert.Equal(20, _sut.MaxQuantityPerProduct);
    }
}
