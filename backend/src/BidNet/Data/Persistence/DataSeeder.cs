using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Data.Persistence;

public class DataSeeder(UserManager<User> userManager, RoleManager<UserRole> roleManager, AppDbContext dbContext)
{
    public async Task SeedAsync()
    {
        // Apply pending migrations
        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        string[] roleNames = [UserRoles.Admin, UserRoles.Seller, UserRoles.Bidder];

        foreach (var roleName in roleNames)
        {
            var exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await roleManager.CreateAsync(new UserRole()
                {
                    Id = new(Guid.NewGuid()),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }

        var admin = await userManager.FindByNameAsync("admin");
        var adminId = admin?.Id ?? new(Guid.NewGuid());

        // Seed initial data if necessary
        if (admin is null)
        {
            var adminUser = new User
            {
                Id = adminId,
                UserName = "admin",
                Email = "admin@example.com"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
            }
            else
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        var sampleAuction = new Auction(
            title: "Sample Auction",
            description: "This is a sample auction for demonstration purposes.",
            startDate: DateTime.UtcNow.AddSeconds(20),
            endDate: DateTime.UtcNow.AddDays(7),
            startingPrice: 100.00m,
            createdBy: adminId);

        sampleAuction.Start();

        dbContext.Auctions.Add(sampleAuction);
        await dbContext.SaveChangesAsync();
    }
}
