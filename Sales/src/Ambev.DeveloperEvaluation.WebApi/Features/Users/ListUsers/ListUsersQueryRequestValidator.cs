using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUsers;

public class ListUsersQueryRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class ListUsersQueryRequestValidator : AbstractValidator<ListUsersQueryRequest>
{
    public ListUsersQueryRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
