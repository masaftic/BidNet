using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new PaymentTransactionId(x));
        
        builder.Property(x => x.UserId)
            .HasConversion(x => x.Value, x => new UserId(x));

        builder.Property(x => x.AuctionId)
            .HasConversion(x => x.Value, x => new AuctionId(x));

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);
    }
}
