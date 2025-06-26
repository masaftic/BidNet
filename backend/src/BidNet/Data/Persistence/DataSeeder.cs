using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Data.Persistence;

public class DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole<UserId>> roleManager, AppDbContext dbContext)
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

        var adminId = new UserId(Guid.NewGuid());

        // Seed initial data if necessary
        if (!await userManager.Users.AnyAsync())
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
            startDate: DateTime.UtcNow,
            endDate: DateTime.UtcNow.AddDays(7),
            startingPrice: 100.00m,
            createdBy: adminId);

        sampleAuction.Start();

        if (!await dbContext.Auctions.AnyAsync(a => a.Title == sampleAuction.Title))
        {
            dbContext.Auctions.Add(sampleAuction);
            await dbContext.SaveChangesAsync();
        }
    }
}
