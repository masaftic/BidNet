using Bidnet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bidnet.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new BidId(x));

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);
        
        builder.HasOne<Auction>()
            .WithMany()
            .HasForeignKey(x => x.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
