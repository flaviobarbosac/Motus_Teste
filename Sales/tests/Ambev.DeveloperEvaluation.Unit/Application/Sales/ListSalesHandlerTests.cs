using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class ListSalesHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<SaleApplicationProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact(DisplayName = "List sales returns page metadata and mapped items")]
    public async Task Handle_ReturnsPagedResults()
    {
        var sales = new List<Sale>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SaleNumber = 1,
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "c",
                BranchId = Guid.NewGuid(),
                BranchName = "b",
                TotalAmount = 1m,
                Items = new List<SaleItem>()
            }
        };

        var repo = Substitute.For<ISaleRepository>();
        repo.CountAsync(Arg.Any<CancellationToken>()).Returns(1);
        repo.ListAsync(1, 20, Arg.Any<CancellationToken>()).Returns(sales);

        var handler = new ListSalesHandler(repo, CreateMapper());
        var result = await handler.Handle(new ListSalesQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].SaleNumber.Should().Be(1);
    }
}
