using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using BidNet.Shared.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Wallets.Queries;

public record GetWalletBalanceQuery : IRequest<ErrorOr<WalletDto>>;

public class GetWalletBalanceQueryHandler : IRequestHandler<GetWalletBalanceQuery, ErrorOr<WalletDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetWalletBalanceQueryHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<WalletDto>> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        var wallet = await _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            // Return an empty wallet if none exists
            wallet = new Wallet(userId);
            await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return wallet.ToWalletDto();
    }
}
