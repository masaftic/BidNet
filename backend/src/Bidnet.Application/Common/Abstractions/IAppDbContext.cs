using Bidnet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bidnet.Application.Common.Abstractions;

public interface IAppDbContext
{
    DbSet<Bid> Bids { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
