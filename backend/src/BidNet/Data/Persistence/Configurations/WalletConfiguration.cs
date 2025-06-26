using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.Id)
            .ValueGeneratedNever();
            
        builder.Property(w => w.Balance)
            .HasPrecision(18, 2);
            
        builder.Property(w => w.HeldBalance)
            .HasPrecision(18, 2);
            
        builder.Property(w => w.Currency)
            .HasMaxLength(3);
            
        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .ValueGeneratedNever();
            
        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);
            
        builder.Property(t => t.Description)
            .HasMaxLength(500);
            
        builder.Property(t => t.Type)
            .HasConversion<string>();
    }
}
