using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Shared.Abstractions;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Queries;

public record GetMyProfileQuery : IRequest<ErrorOr<User>>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ErrorOr<User>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileQueryHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<User>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);
        
        if (user is null)
        {
            return Error.NotFound(description: "User not found");
        }

        return user;
    }
}
