using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersQuery : IRequest<ListUsersResult>
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
