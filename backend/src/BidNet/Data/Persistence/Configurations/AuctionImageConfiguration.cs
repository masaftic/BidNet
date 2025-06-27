using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class AuctionImageConfiguration : IEntityTypeConfiguration<AuctionImage>
{
    public void Configure(EntityTypeBuilder<AuctionImage> builder)
    {
        builder.HasKey(a => a.Id);
            
        builder.Property(a => a.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(a => a.IsPrimary)
            .IsRequired();
            
        builder.Property(a => a.UploadedAt)
            .IsRequired();
            
        builder.HasOne(a => a.Auction)
            .WithMany(a => a.Images)
            .HasForeignKey(a => a.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(a => a.AuctionId);
    }
}
