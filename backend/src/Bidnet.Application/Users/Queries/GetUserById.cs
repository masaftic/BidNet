using Bidnet.Application.Common.Abstractions;
using Bidnet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bidnet.Application.Users.Queries;

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
    private readonly IAppDbContext _dbContext;

    public GetUserByIdQueryHandler(IAppDbContext dbContext)
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
