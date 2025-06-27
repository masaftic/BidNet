using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Mapping;
using BidNet.Features.Auctions.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Queries;

public class GetAuctionByIdQueryValidator : AbstractValidator<GetAuctionByIdQuery>
{
    public GetAuctionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

public record GetAuctionByIdQuery(AuctionId Id) : IRequest<ErrorOr<AuctionDto>>;

public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, ErrorOr<AuctionDto>>
{
    private readonly AppDbContext _dbContext;

    public GetAuctionByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<AuctionDto>> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext
            .Auctions
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .ToAuctionDto()
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found.");
        }

        return auction;
    }
}
