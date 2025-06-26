using BidNet.Data.Persistence;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Wallets.Commands;

public record WithdrawFundsCommand(decimal Amount, string WithdrawalMethod, string WithdrawalDetails) 
    : IRequest<ErrorOr<WalletDto>>;

public class WithdrawFundsCommandValidator : AbstractValidator<WithdrawFundsCommand>
{
    public WithdrawFundsCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Withdrawal amount must be greater than zero");
        
        RuleFor(x => x.WithdrawalMethod)
            .NotEmpty().WithMessage("Withdrawal method is required");
    }
}

public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand, ErrorOr<WalletDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public WithdrawFundsCommandHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<WalletDto>> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        var wallet = await _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            return Error.NotFound(description: "Wallet not found");
        }

        var description = $"Withdrawal via {request.WithdrawalMethod}";
        var result = wallet.Withdraw(request.Amount, description);
        
        if (result.IsError)
        {
            return result.Errors;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return wallet.ToWalletDto();
    }
}
