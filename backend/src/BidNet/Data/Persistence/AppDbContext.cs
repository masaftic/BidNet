using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Data.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Bid> Bids { get; init; }
    public DbSet<Auction> Auctions { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
