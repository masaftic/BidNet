using BidNet.Data.Persistence;
using BidNet.Features.Bids.Mapping;
using BidNet.Features.Bids.Models;
using BidNet.Shared.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Bids.Queries;


public record GetMyBidsQuery : IRequest<ErrorOr<UserBidsResponse>>;

public class GetMyBidsQueryHandler : IRequestHandler<GetMyBidsQuery, ErrorOr<UserBidsResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyBidsQueryHandler(
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<UserBidsResponse>> Handle(GetMyBidsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return Error.NotFound(description: "User not found");
        }

        var bids = await _dbContext.Bids
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .ToBidDto()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return new UserBidsResponse
        {
            UserId = user.Id,
            Bids = bids
        };
    }
}
