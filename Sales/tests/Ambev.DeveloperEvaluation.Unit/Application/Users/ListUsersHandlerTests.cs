using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class ListUsersHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<GetUserProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact(DisplayName = "List users returns page metadata and mapped items")]
    public async Task Handle_ReturnsPagedResults()
    {
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Username = "alice",
                Email = "alice@test.local",
                Phone = "+5511987654321",
                Password = "x",
                Role = UserRole.Customer,
                Status = UserStatus.Active
            }
        };

        var repo = Substitute.For<IUserRepository>();
        repo.CountAsync(Arg.Any<CancellationToken>()).Returns(1);
        repo.ListAsync(1, 20, Arg.Any<CancellationToken>()).Returns(users);

        var handler = new ListUsersHandler(repo, CreateMapper());
        var result = await handler.Handle(new ListUsersQuery { Page = 1, PageSize = 20 }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("alice");
        result.Items[0].Email.Should().Be("alice@test.local");
    }
}
