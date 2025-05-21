using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new BidId(x));

        builder.Property(x => x.AuctionId)
            .HasConversion(x => x.Value, x => new AuctionId(x));

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);
        
        builder.HasOne(x => x.Auction)
            .WithMany()
            .HasForeignKey(x => x.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
