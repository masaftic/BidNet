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

public record TransferFundsCommand(UserId RecipientId, decimal Amount, string Description)
    : IRequest<ErrorOr<WalletDto>>;

public class TransferFundsCommandValidator : AbstractValidator<TransferFundsCommand>
{
    public TransferFundsCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer amount must be greater than zero");

        RuleFor(x => x.RecipientId)
            .NotEqual(default(UserId)).WithMessage("Recipient is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }
}

public class TransferFundsCommandHandler : IRequestHandler<TransferFundsCommand, ErrorOr<WalletDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public TransferFundsCommandHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<WalletDto>> Handle(TransferFundsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        using var trx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var senderWallet = await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (senderWallet == null)
            {
                return Error.NotFound(description: "Sender wallet not found");
            }

            // Check if recipient exists
            var recipientExists = await _dbContext.Users.AnyAsync(u => u.Id == request.RecipientId, cancellationToken);
            if (!recipientExists)
            {
                return Error.NotFound(description: "Recipient not found");
            }

            // Get or create recipient wallet
            var recipientWallet = await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == request.RecipientId, cancellationToken);

            if (recipientWallet == null)
            {
                recipientWallet = new Wallet(request.RecipientId);
                await _dbContext.Wallets.AddAsync(recipientWallet, cancellationToken);
            }

            // Execute the transfer
            var transferResult = senderWallet.Transfer(request.Amount, request.RecipientId, request.Description);
            if (transferResult.IsError)
            {
                return transferResult.Errors;
            }

            // Add funds to recipient
            var depositResult = recipientWallet.Deposit(request.Amount, $"Transfer from {userId}");
            if (depositResult.IsError)
            {
                return depositResult.Errors;
            }

            await trx.CommitAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return senderWallet.ToWalletDto();
        }
        catch (Exception)
        {
            await trx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
