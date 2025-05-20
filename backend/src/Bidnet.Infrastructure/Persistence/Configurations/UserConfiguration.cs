using Bidnet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bidnet.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new UserId(x));
        
        builder.Property(x => x.Role)
            .HasConversion<string>()
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
