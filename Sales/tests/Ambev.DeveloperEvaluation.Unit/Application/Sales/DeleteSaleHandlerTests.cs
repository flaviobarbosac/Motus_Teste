using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
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
        repo.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new DeleteSaleHandler(repo);
        var result = await handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact(DisplayName = "Delete sale throws when not found")]
    public async Task Handle_Missing_ThrowsKeyNotFoundException()
    {
        var repo = Substitute.For<ISaleRepository>();
        repo.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var handler = new DeleteSaleHandler(repo);
        var act = () => handler.Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
