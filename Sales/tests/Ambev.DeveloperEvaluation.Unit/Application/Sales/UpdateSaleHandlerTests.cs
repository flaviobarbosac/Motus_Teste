using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class UpdateSaleHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<SaleApplicationProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact(DisplayName = "Update sale throws when duplicate sale number belongs to another sale")]
    public async Task Handle_DuplicateSaleNumberOnOtherSale_Throws()
    {
        var id = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var repo = Substitute.For<ISaleRepository>();
        var events = Substitute.For<ISaleLifecycleEvents>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(new Sale { Id = id, SaleNumber = 1 });
        repo.GetBySaleNumberAsync(99, Arg.Any<CancellationToken>())
            .Returns(new Sale { Id = otherId, SaleNumber = 99 });

        var handler = new UpdateSaleHandler(repo, new SaleLineDiscountCalculator(), events, CreateMapper());
        var command = new UpdateSaleCommand
        {
            Id = id,
            SaleNumber = 99,
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
        events.DidNotReceive().SaleModified(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<DateTime>());
    }

    [Fact(DisplayName = "Update sale allows keeping same sale number")]
    public async Task Handle_SameSaleNumber_Updates()
    {
        var id = Guid.NewGuid();
        var existing = new Sale
        {
            Id = id,
            SaleNumber = 5,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "c",
            BranchId = Guid.NewGuid(),
            BranchName = "b",
            TotalAmount = 0,
            IsCancelled = false,
            Items = new List<SaleItem>()
        };

        var repo = Substitute.For<ISaleRepository>();
        var events = Substitute.For<ISaleLifecycleEvents>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        repo.GetBySaleNumberAsync(5, Arg.Any<CancellationToken>()).Returns(existing);
        repo.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());

        var handler = new UpdateSaleHandler(repo, new SaleLineDiscountCalculator(), events, CreateMapper());
        var command = new UpdateSaleCommand
        {
            Id = id,
            SaleNumber = 5,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "c2",
            BranchId = Guid.NewGuid(),
            BranchName = "b2",
            IsCancelled = false,
            Items = new List<CreateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductDescription = "p", Quantity = 2, UnitPrice = 10m }
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.CustomerName.Should().Be("c2");
        await repo.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        events.Received(1).SaleModified(id, 5, Arg.Any<decimal>(), Arg.Any<DateTime>());
        events.DidNotReceive().SaleCancelled(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>());
    }

    [Fact(DisplayName = "Update sale emits SaleCancelled when header becomes cancelled")]
    public async Task Handle_SetsCancelled_EmitsSaleCancelled()
    {
        var id = Guid.NewGuid();
        var existing = new Sale
        {
            Id = id,
            SaleNumber = 7,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "c",
            BranchId = Guid.NewGuid(),
            BranchName = "b",
            TotalAmount = 10m,
            IsCancelled = false,
            Items = new List<SaleItem>()
        };

        var repo = Substitute.For<ISaleRepository>();
        var events = Substitute.For<ISaleLifecycleEvents>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existing);
        repo.GetBySaleNumberAsync(7, Arg.Any<CancellationToken>()).Returns(existing);
        repo.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());

        var handler = new UpdateSaleHandler(repo, new SaleLineDiscountCalculator(), events, CreateMapper());
        var command = new UpdateSaleCommand
        {
            Id = id,
            SaleNumber = 7,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "c",
            BranchId = Guid.NewGuid(),
            BranchName = "b",
            IsCancelled = true,
            Items = new List<CreateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductDescription = "p", Quantity = 1, UnitPrice = 10m }
            }
        };

        await handler.Handle(command, CancellationToken.None);

        events.Received(1).SaleCancelled(id, 7, "SaleMarkedCancelled", Arg.Any<DateTime>());
    }
}
