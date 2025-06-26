namespace BidNet.Features.Wallets.Models;

public class WalletDto
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Deposit, Withdrawal, Hold, Release
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DepositRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "CreditCard"; // Could be CreditCard, BankTransfer, etc.
    public string PaymentDetails { get; set; } = string.Empty; // JSON or other format containing payment details
}

public class WithdrawalRequest
{
    public decimal Amount { get; set; }
    public string WithdrawalMethod { get; set; } = "BankTransfer"; // BankTransfer, PayPal, etc.
    public string WithdrawalDetails { get; set; } = string.Empty; // Account details, etc.
}

public class TransferFundsRequest
{
    public Guid RecipientId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TransactionHistoryResponse
{
    public Guid UserId { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// DTO for real-time wallet balance notifications
public record WalletBalanceNotificationDto(
    Guid UserId,
    Guid WalletId,
    decimal Balance,
    decimal AvailableBalance,
    decimal HeldAmount,
    string Message,
    DateTime Timestamp
);
