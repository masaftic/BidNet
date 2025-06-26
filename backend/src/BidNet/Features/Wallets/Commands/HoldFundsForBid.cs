using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Wallets.Commands;

public record HoldFundsForBidCommand(AuctionId AuctionId, decimal Amount) : IRequest<ErrorOr<WalletDto>>;

public class HoldFundsForBidCommandValidator : AbstractValidator<HoldFundsForBidCommand>
{
    public HoldFundsForBidCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than zero");
            
        RuleFor(x => x.AuctionId)
            .NotEqual(default(AuctionId)).WithMessage("Auction ID is required");
    }
}

public class HoldFundsForBidCommandHandler : IRequestHandler<HoldFundsForBidCommand, ErrorOr<WalletDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public HoldFundsForBidCommandHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<WalletDto>> Handle(HoldFundsForBidCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        // Get auction to verify it exists
        var auction = await _dbContext.Auctions
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);
            
        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found");
        }
        
        var wallet = await _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            return Error.NotFound(description: "Wallet not found. Please deposit funds first.");
        }

        var description = $"Hold for bid on auction {request.AuctionId}";
        var result = wallet.Hold(request.Amount, description);
        
        if (result.IsError)
        {
            return result.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet.ToWalletDto();
    }
}
