using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Auctions.Commands;

public class DeleteAuctionCommandValidator : AbstractValidator<DeleteAuctionCommand>
{
    public DeleteAuctionCommandValidator()
    {
        RuleFor(x => x.Id.Value)
            .NotEmpty();
    }
}

public record DeleteAuctionCommand(AuctionId Id) : IRequest<ErrorOr<Deleted>>;

public class DeleteAuctionCommandHandler : IRequestHandler<DeleteAuctionCommand, ErrorOr<Deleted>>
{
    private readonly AppDbContext _dbContext;

    public DeleteAuctionCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions.FindAsync(request.Id);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found.");
        }

        _dbContext.Auctions.Remove(auction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
