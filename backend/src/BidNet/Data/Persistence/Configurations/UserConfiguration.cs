using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BidNet.Data.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new UserId(x));
        
        builder.Property(x => x.Roles)
            .HasConversion<string>(
                x => string.Join(',', x),
                x => x.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(role => Enum.Parse<UserRole>(role))
                      .ToHashSet()
            )
            .IsRequired();

        builder.Property(x => x.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
