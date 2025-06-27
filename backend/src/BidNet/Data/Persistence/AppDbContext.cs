using BidNet.Data.Persistence.Converters;
using BidNet.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Data.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, UserRole, UserId>(options)
{
    public DbSet<Bid> Bids { get; init; }
    public DbSet<Auction> Auctions { get; init; }
    public DbSet<AuctionImage> AuctionImages { get; init; }
    public DbSet<AuctionEventLog> AuctionEventLogs { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    public DbSet<Wallet> Wallets { get; init; }
    public DbSet<WalletTransaction> WalletTransactions { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<UserId>()
            .HaveConversion<UserIdConverter>();

        configurationBuilder
            .Properties<AuctionId>()
            .HaveConversion<AuctionIdConverter>();
            
        configurationBuilder
            .Properties<AuctionImageId>()
            .HaveConversion<AuctionImageIdConverter>();

        configurationBuilder
            .Properties<BidId>()
            .HaveConversion<BidIdConverter>();

        configurationBuilder
            .Properties<RefreshTokenId>()
            .HaveConversion<RefreshTokenIdConverter>();

        configurationBuilder
            .Properties<AuctionEventLogId>()
            .HaveConversion<AuctionEventLogIdConverter>();

        configurationBuilder
            .Properties<PaymentTransactionId>()
            .HaveConversion<PaymentTransactionIdConverter>();

        configurationBuilder
            .Properties<WalletId>()
            .HaveConversion<WalletIdConverter>();

        configurationBuilder
            .Properties<WalletTransactionId>()
            .HaveConversion<WalletTransactionIdConverter>();
    }
}

