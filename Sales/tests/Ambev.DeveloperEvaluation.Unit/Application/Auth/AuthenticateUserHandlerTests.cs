using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Auth;

public class AuthenticateUserHandlerTests
{
    [Fact]
    public async Task Handle_ValidActiveUser_ReturnsTokenAndProfile()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@test.local",
            Phone = "+5511987654321",
            Password = "stored-hash",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var repo = Substitute.For<IUserRepository>();
        repo.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword("plain", user.Password).Returns(true);

        var jwt = Substitute.For<IJwtTokenGenerator>();
        jwt.GenerateToken(user).Returns("jwt-token");

        var handler = new AuthenticateUserHandler(repo, hasher, jwt);
        var result = await handler.Handle(
            new AuthenticateUserCommand { Email = user.Email, Password = "plain" },
            CancellationToken.None);

        Assert.Equal("jwt-token", result.Token);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Username, result.Name);
        Assert.Equal(user.Role.ToString(), result.Role);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        var repo = Substitute.For<IUserRepository>();
        repo.GetByEmailAsync("missing@test.local", Arg.Any<CancellationToken>()).Returns((User?)null);

        var handler = new AuthenticateUserHandler(repo, Substitute.For<IPasswordHasher>(), Substitute.For<IJwtTokenGenerator>());

        var act = () => handler.Handle(
            new AuthenticateUserCommand { Email = "missing@test.local", Password = "x" },
            CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "u",
            Email = "u@test.local",
            Phone = "+5511987654321",
            Password = "hash",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };
        var repo = Substitute.For<IUserRepository>();
        repo.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword("wrong", user.Password).Returns(false);

        var handler = new AuthenticateUserHandler(repo, hasher, Substitute.For<IJwtTokenGenerator>());

        var act = () => handler.Handle(
            new AuthenticateUserCommand { Email = user.Email, Password = "wrong" },
            CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "inactive",
            Email = "in@test.local",
            Phone = "+5511987654321",
            Password = "hash",
            Role = UserRole.Customer,
            Status = UserStatus.Inactive
        };
        var repo = Substitute.For<IUserRepository>();
        repo.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword("plain", user.Password).Returns(true);

        var handler = new AuthenticateUserHandler(repo, hasher, Substitute.For<IJwtTokenGenerator>());

        var act = () => handler.Handle(
            new AuthenticateUserCommand { Email = user.Email, Password = "plain" },
            CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
    }
}
