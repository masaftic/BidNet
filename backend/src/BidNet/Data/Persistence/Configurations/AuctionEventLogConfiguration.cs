using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class AuctionEventLogConfiguration : IEntityTypeConfiguration<AuctionEventLog>
{
    public void Configure(EntityTypeBuilder<AuctionEventLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Details)
            .HasMaxLength(1000)
            .IsRequired();
    }
}
