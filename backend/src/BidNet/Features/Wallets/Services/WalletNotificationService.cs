using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Hubs;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using Microsoft.AspNetCore.SignalR;

namespace BidNet.Features.Wallets.Services;

public interface IWalletNotificationService
{
    Task NotifyBalanceChanged(Wallet wallet, string message);
    Task NotifyTransactionOccurred(WalletTransaction transaction);
}

public class WalletNotificationService : IWalletNotificationService
{
    private readonly IHubContext<WalletHub> _hubContext;
    private readonly ILogger<WalletNotificationService> _logger;

    public WalletNotificationService(
        IHubContext<WalletHub> hubContext,
        ILogger<WalletNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyBalanceChanged(Wallet wallet, string message)
    {
        try
        {
            var walletDto = wallet.ToWalletDto();
            
            // Create notification data with balance and message
            var notification = new WalletBalanceNotificationDto(
                UserId: wallet.UserId.Value,
                WalletId: wallet.Id,
                Balance: wallet.Balance,
                AvailableBalance: wallet.Balance - wallet.HeldBalance,
                HeldAmount: wallet.HeldBalance,
                Message: message,
                Timestamp: DateTime.UtcNow
            );
            
            // Send to specific user's wallet group
            await _hubContext.Clients
                .Group($"user-wallet-{wallet.UserId.Value}")
                .SendAsync("WalletBalanceChanged", notification);
            
            _logger.LogInformation("Wallet balance change notification sent to user {UserId}", wallet.UserId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying wallet balance change for user {UserId}", wallet.UserId.Value);
        }
    }

    public async Task NotifyTransactionOccurred(WalletTransaction transaction)
    {
        try
        {
            var transactionDto = transaction.ToTransactionDto();
            
            // Send to specific user's wallet group
            await _hubContext.Clients
                .Group($"user-wallet-{transaction.UserId.Value}")
                .SendAsync("WalletTransactionOccurred", transactionDto);
            
            _logger.LogInformation("Wallet transaction notification sent to user {UserId}", 
                transaction.UserId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying wallet transaction for user {UserId}", 
                transaction.UserId.Value);
        }
    }
}
