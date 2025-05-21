using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Auctions.Queries;

public class GetAuctionByIdQueryValidator : AbstractValidator<GetAuctionByIdQuery>
{
    public GetAuctionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

public record GetAuctionByIdQuery(Guid Id) : IRequest<ErrorOr<Auction>>;

public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, ErrorOr<Auction>>
{
    private readonly AppDbContext _dbContext;

    public GetAuctionByIdQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<Auction>> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions.FindAsync(request.Id);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found.");
        }

        return auction;
    }
}
