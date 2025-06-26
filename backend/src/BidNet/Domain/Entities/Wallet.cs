using BidNet.Domain.Enums;
using ErrorOr;

namespace BidNet.Domain.Entities;

public readonly record struct WalletId(Guid Value)
{
    public static WalletId New() => new(Guid.NewGuid());
    
    public static implicit operator Guid(WalletId id) => id.Value;
    public static implicit operator WalletId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class Wallet
{
    public WalletId Id { get; private set; }
    public UserId UserId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal HeldBalance { get; private set; }
    public string Currency { get; private set; }
    public List<WalletTransaction> Transactions { get; private set; } = new();

    private Wallet()
    {
        Currency = "USD";
    }

    public Wallet(UserId userId, string currency = "USD")
    {
        Id = WalletId.New();
        UserId = userId;
        Balance = 0;
        HeldBalance = 0;
        Currency = currency;
    }

    public ErrorOr<Success> Deposit(decimal amount, string description)
    {
        if (amount <= 0)
            return Error.Validation(description: "Amount must be greater than zero");

        Balance += amount;

        var transaction = new WalletTransaction(
            Id,
            UserId,
            TransactionType.Deposit,
            amount,
            description,
            DateTime.UtcNow);

        Transactions.Add(transaction);
        return Result.Success;
    }

    public ErrorOr<Success> Withdraw(decimal amount, string description)
    {
        if (amount <= 0)
            return Error.Validation(description: "Amount must be greater than zero");

        if (amount > Balance - HeldBalance)
            return Error.Validation(description: "Insufficient funds");

        Balance -= amount;

        var transaction = new WalletTransaction(
            Id,
            UserId,
            TransactionType.Withdrawal,
            amount,
            description,
            DateTime.UtcNow);

        Transactions.Add(transaction);
        return Result.Success;
    }

    public ErrorOr<Success> Hold(decimal amount, string description)
    {
        if (amount <= 0)
            return Error.Validation(description: "Amount must be greater than zero");

        if (amount > Balance - HeldBalance)
            return Error.Validation(description: "Insufficient funds");

        HeldBalance += amount;

        var transaction = new WalletTransaction(
            Id,
            UserId,
            TransactionType.Hold,
            amount,
            description,
            DateTime.UtcNow);

        Transactions.Add(transaction);
        return Result.Success;
    }

    public ErrorOr<Success> Release(decimal amount, string description)
    {
        if (amount <= 0)
            return Error.Validation(description: "Amount must be greater than zero");

        if (amount > HeldBalance)
            return Error.Validation(description: "Amount exceeds held balance");

        HeldBalance -= amount;

        var transaction = new WalletTransaction(
            Id,
            UserId,
            TransactionType.Release,
            amount,
            description,
            DateTime.UtcNow);

        Transactions.Add(transaction);
        return Result.Success;
    }

    public ErrorOr<Success> Transfer(decimal amount, UserId recipientId, string description)
    {
        if (amount <= 0)
            return Error.Validation(description: "Amount must be greater than zero");

        if (amount > Balance - HeldBalance)
            return Error.Validation(description: "Insufficient funds");

        Balance -= amount;

        var transaction = new WalletTransaction(
            Id,
            UserId,
            TransactionType.Transfer,
            amount,
            description,
            DateTime.UtcNow,
            recipientId);

        Transactions.Add(transaction);
        return Result.Success;
    }

    public decimal GetAvailableBalance() => Balance - HeldBalance;
}
