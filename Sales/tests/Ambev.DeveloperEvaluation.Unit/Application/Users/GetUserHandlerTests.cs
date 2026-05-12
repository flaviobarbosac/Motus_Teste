using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class GetUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidId_ReturnsMappedUser()
    {
        var id = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            Username = "bob",
            Email = "bob@test.local",
            Phone = "+5511987654321",
            Password = "hash",
            Role = UserRole.Manager,
            Status = UserStatus.Active
        };
        var expected = new GetUserResult { Id = id, Name = "bob", Email = user.Email, Phone = user.Phone, Role = user.Role, Status = user.Status };

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        var mapper = Substitute.For<IMapper>();
        mapper.Map<GetUserResult>(user).Returns(expected);

        var handler = new GetUserHandler(repo, mapper);
        var result = await handler.Handle(new GetUserCommand(id), CancellationToken.None);

        result.Should().BeSameAs(expected);
        await repo.Received(1).GetByIdAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((User?)null);
        var mapper = Substitute.For<IMapper>();
        var handler = new GetUserHandler(repo, mapper);

        var act = () => handler.Handle(new GetUserCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .Where(e => e.Message.Contains(id.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public async Task Handle_EmptyId_ThrowsValidationException()
    {
        var repo = Substitute.For<IUserRepository>();
        var mapper = Substitute.For<IMapper>();
        var handler = new GetUserHandler(repo, mapper);

        var act = () => handler.Handle(new GetUserCommand(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
