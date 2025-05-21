using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class AuctionEventLogConfiguration : IEntityTypeConfiguration<AuctionEventLog>
{
    public void Configure(EntityTypeBuilder<AuctionEventLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new AuctionEventLogId(x));

        builder.Property(x => x.AuctionId)
            .HasConversion(x => x.Value, x => new AuctionId(x));

        builder.Property(x => x.Details)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
