using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using BidNet.Features.Wallets.Services;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Wallets.Commands;

public record DepositFundsCommand(decimal Amount, string PaymentMethod, string PaymentDetails) : IRequest<ErrorOr<WalletDto>>;

public class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommand>
{
    public DepositFundsCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Deposit amount must be greater than zero");
        
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required");
    }
}

public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand, ErrorOr<WalletDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWalletNotificationService _notificationService;

    public DepositFundsCommandHandler(
        AppDbContext dbContext, 
        ICurrentUserService currentUserService,
        IWalletNotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<ErrorOr<WalletDto>> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        var wallet = await _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            wallet = new Wallet(userId);
            await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
        }

        var description = $"Deposit via {request.PaymentMethod}";
        var result = wallet.Deposit(request.Amount, description);
        
        if (result.IsError)
        {
            return result.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // Get the latest transaction that was created
        var latestTransaction = wallet.Transactions.OrderByDescending(t => t.Timestamp).First();
        
        // Send notifications
        await _notificationService.NotifyBalanceChanged(wallet, $"Deposited {request.Amount:C}");
        await _notificationService.NotifyTransactionOccurred(latestTransaction);
        
        return wallet.ToWalletDto();
    }
}
