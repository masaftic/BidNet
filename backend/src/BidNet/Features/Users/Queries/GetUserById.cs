using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Users.Queries;

public class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}

public record GetUserByIdQuery(UserId UserId) : IRequest<ErrorOr<User>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ErrorOr<User>>
{
    private readonly AppDbContext _dbContext;

    public GetUserByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<User>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FindAsync(request.UserId.Value);

        if (user is null)
        {
            return Error.NotFound(description: "User not found");
        }

        return user;
    }
}
