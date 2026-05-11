using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class GetSaleHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<SaleApplicationProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact(DisplayName = "Get sale returns mapped result when found")]
    public async Task Handle_ExistingSale_ReturnsResult()
    {
        var id = Guid.NewGuid();
        var sale = new Sale
        {
            Id = id,
            SaleNumber = 10,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "A",
            BranchId = Guid.NewGuid(),
            BranchName = "B",
            TotalAmount = 50m,
            IsCancelled = false,
            Items = new List<SaleItem>()
        };

        var repo = Substitute.For<ISaleRepository>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(sale);

        var handler = new GetSaleHandler(repo, CreateMapper());
        var result = await handler.Handle(new GetSaleCommand(id), CancellationToken.None);

        result.Id.Should().Be(id);
        result.SaleNumber.Should().Be(10);
    }

    [Fact(DisplayName = "Get sale throws when missing")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var repo = Substitute.For<ISaleRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var handler = new GetSaleHandler(repo, CreateMapper());
        var act = () => handler.Handle(new GetSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
