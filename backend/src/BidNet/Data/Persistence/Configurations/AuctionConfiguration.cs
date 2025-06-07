using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new AuctionId(x));

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Winner)
            .WithMany()
            .HasForeignKey(x => x.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StartDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.StartingPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrentPrice)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
