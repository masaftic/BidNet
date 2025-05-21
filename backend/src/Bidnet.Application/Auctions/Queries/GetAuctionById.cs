using Bidnet.Application.Common.Abstractions;
using Bidnet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Bidnet.Application.Auctions.Queries;

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
    private readonly IAppDbContext _dbContext;

    public GetAuctionByIdQueryHandler(IAppDbContext dbContext)
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
