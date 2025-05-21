using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Data.Persistence;

public class DataSeeder
{
    private readonly AppDbContext _dbContext;

    public DataSeeder(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        // Apply pending migrations
        if ((await _dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            await _dbContext.Database.MigrateAsync();
        }

        // Seed initial data if necessary
        if (!await _dbContext.Users.AnyAsync())
        {
            var adminUser = new User(
                username: "admin",
                email: "admin@bidnet.com",
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                role: UserRole.Admin
            );

            _dbContext.Users.Add(adminUser);
            await _dbContext.SaveChangesAsync();
        }
    }
}
