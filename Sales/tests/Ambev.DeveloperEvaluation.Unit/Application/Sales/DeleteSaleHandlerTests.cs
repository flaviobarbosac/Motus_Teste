using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class DeleteSaleHandlerTests
{
    [Fact(DisplayName = "Delete sale succeeds when repository removes row")]
    public async Task Handle_Existing_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var repo = Substitute.For<ISaleRepository>();
        var events = Substitute.For<ISaleLifecycleEvents>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(new Sale { Id = id, SaleNumber = 42 });
        repo.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new DeleteSaleHandler(repo, events);
        var result = await handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
        events.Received(1).SaleCancelled(id, 42, "SaleDeleted", Arg.Any<DateTime>());
    }

    [Fact(DisplayName = "Delete sale throws when not found")]
    public async Task Handle_Missing_ThrowsKeyNotFoundException()
    {
        var repo = Substitute.For<ISaleRepository>();
        var events = Substitute.For<ISaleLifecycleEvents>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var handler = new DeleteSaleHandler(repo, events);
        var act = () => handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        events.DidNotReceive().SaleCancelled(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>());
    }
}
