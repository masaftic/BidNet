using BidNet.Data.Persistence;
using BidNet.Features.Auctions.Mapping;
using BidNet.Features.Auctions.Models;
using BidNet.Shared.Models;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Queries;

public record GetAuctionsListQuery : IRequest<ErrorOr<PaginatedResponse<AuctionDto>>>, IPaginatedRequest
{
    public int PageIndex { get; init; } = 0;
    public int PageSize { get; init; } = 10;
}

public class GetAuctionsListQueryHandler : IRequestHandler<GetAuctionsListQuery, ErrorOr<PaginatedResponse<AuctionDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetAuctionsListQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<PaginatedResponse<AuctionDto>>> Handle(GetAuctionsListQuery request, CancellationToken cancellationToken)
    {
        var auctions = await _dbContext
                    .Auctions
                    .AsNoTracking()
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip(request.PageIndex * request.PageSize)
                    .Take(request.PageSize)
                    .ToAuctionDto()
                    .ToListAsync(cancellationToken: cancellationToken);
        
        var totalCount = await _dbContext.Auctions.CountAsync(cancellationToken: cancellationToken);
        var paginatedResponse = new PaginatedResponse<AuctionDto>
        {
            Items = auctions.ToArray(),
            PageIndex = request.PageIndex,
            TotalCount = totalCount,
            PageSize = request.PageSize
        };

        return paginatedResponse;
    }
}
