using BidNet.Data.Persistence.Configurations;
using BidNet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BidNet.Data.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, IdentityRole<UserId>, UserId>(options)
{
    public DbSet<Bid> Bids { get; init; }
    public DbSet<Auction> Auctions { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }

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
    }
}

