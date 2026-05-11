using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class CreateSaleHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<SaleApplicationProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact(DisplayName = "Create sale with valid data persists aggregate with computed total")]
    public async Task Handle_ValidRequest_CallsRepositoryWithDiscountedTotal()
    {
        var repo = Substitute.For<ISaleRepository>();
        repo.GetBySaleNumberAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        repo.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var handler = new CreateSaleHandler(repo, new SaleLineDiscountCalculator(), CreateMapper());

        var command = new CreateSaleCommand
        {
            SaleNumber = 1001,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Cliente",
            BranchId = Guid.NewGuid(),
            BranchName = "Filial",
            IsCancelled = false,
            Items = new List<CreateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductDescription = "Produto",
                    Quantity = 4,
                    UnitPrice = 100m
                }
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.SaleNumber.Should().Be(1001);
        result.TotalAmount.Should().Be(360m);
        await repo.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.TotalAmount == 360m && s.Items.Count == 1 && s.Items.First().DiscountAmount == 40m),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Create sale with duplicate sale number throws")]
    public async Task Handle_DuplicateSaleNumber_ThrowsInvalidOperationException()
    {
        var repo = Substitute.For<ISaleRepository>();
        repo.GetBySaleNumberAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Sale { Id = Guid.NewGuid(), SaleNumber = 1 });

        var handler = new CreateSaleHandler(repo, new SaleLineDiscountCalculator(), CreateMapper());
        var command = new CreateSaleCommand
        {
            SaleNumber = 1,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "c",
            BranchId = Guid.NewGuid(),
            BranchName = "b",
            Items = new List<CreateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductDescription = "p", Quantity = 1, UnitPrice = 1m }
            }
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "Create sale without items throws validation exception")]
    public async Task Handle_NoItems_ThrowsValidationException()
    {
        var repo = Substitute.For<ISaleRepository>();
        var handler = new CreateSaleHandler(repo, new SaleLineDiscountCalculator(), CreateMapper());
        var command = new CreateSaleCommand
        {
            SaleNumber = 2,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "c",
            BranchId = Guid.NewGuid(),
            BranchName = "b",
            Items = new List<CreateSaleItemCommand>()
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
