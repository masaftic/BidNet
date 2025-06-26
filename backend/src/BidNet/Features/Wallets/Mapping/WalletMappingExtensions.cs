using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Models;

namespace BidNet.Features.Wallets.Mapping;

public static class WalletMappingExtensions
{
    public static WalletDto ToWalletDto(this Wallet wallet, int recentTransactionCount = 5)
    {
        return new WalletDto
        {
            UserId = wallet.UserId,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            RecentTransactions = wallet.Transactions
                .OrderByDescending(t => t.Timestamp)
                .Take(recentTransactionCount)
                .Select(t => t.ToTransactionDto())
                .ToList()
        };
    }

    public static TransactionDto ToTransactionDto(this WalletTransaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            Type = transaction.Type.ToString(),
            Amount = transaction.Amount,
            Description = transaction.Description,
            Timestamp = transaction.Timestamp
        };
    }
}
