using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Queries;

public class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidator()
    {
        // Validation for pagination parameters could be added here
    }
}

public record ListUsersQuery : IRequest<ErrorOr<List<User>>>;

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, ErrorOr<List<User>>>
{
    private readonly AppDbContext _dbContext;

    public ListUsersQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<List<User>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users.ToListAsync(cancellationToken);
        return users;
    }
}
