using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class DeleteUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidId_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var repo = Substitute.For<IUserRepository>();
        repo.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new DeleteUserHandler(repo);
        var result = await handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        result.Success.Should().BeTrue();
        await repo.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var repo = Substitute.For<IUserRepository>();
        repo.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new DeleteUserHandler(repo);

        var act = () => handler.Handle(new DeleteUserCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .Where(e => e.Message.Contains(id.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        var repo = Substitute.For<IUserRepository>();
        var handler = new DeleteUserHandler(repo);

        var act = () => handler.Handle(new DeleteUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
