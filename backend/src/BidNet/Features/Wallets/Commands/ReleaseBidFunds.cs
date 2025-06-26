using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Wallets.Commands;

public record ReleaseBidFundsCommand(AuctionId AuctionId, UserId BidderId, decimal Amount) : IRequest<ErrorOr<WalletDto>>;

public class ReleaseBidFundsCommandValidator : AbstractValidator<ReleaseBidFundsCommand>
{
    public ReleaseBidFundsCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than zero");
            
        RuleFor(x => x.AuctionId)
            .NotEqual(default(AuctionId)).WithMessage("Auction ID is required");
            
        RuleFor(x => x.BidderId)
            .NotEqual(default(UserId)).WithMessage("Bidder ID is required");
    }
}

public class ReleaseBidFundsCommandHandler : IRequestHandler<ReleaseBidFundsCommand, ErrorOr<WalletDto>>
{
    private readonly AppDbContext _dbContext;

    public ReleaseBidFundsCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<WalletDto>> Handle(ReleaseBidFundsCommand request, CancellationToken cancellationToken)
    {
        // Get auction to verify it exists
        var auction = await _dbContext.Auctions
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);
            
        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }
        
        var wallet = await _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == request.BidderId, cancellationToken);

        if (wallet == null)
        {
            return Error.NotFound(description: "Wallet not found");
        }

        var description = $"Release from outbid on auction {request.AuctionId}";
        var result = wallet.Release(request.Amount, description);
        
        if (result.IsError)
        {
            return result.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet.ToWalletDto();
    }
}
