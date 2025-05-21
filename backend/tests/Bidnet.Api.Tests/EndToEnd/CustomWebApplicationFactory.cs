using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using BidNet.Data.Persistence;

namespace Bidnet.Api.Tests.EndToEnd;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Ensure no other database providers are registered
            var dbProviderDescriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList();
            foreach (var dbDescriptor in dbProviderDescriptors)
            {
                services.Remove(dbDescriptor);
            }

            // Add a new in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create the database and seed data
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();
                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

                db.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data
                    var dataSeeder = scopedServices.GetRequiredService<DataSeeder>();
                    dataSeeder.SeedAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test data. Error: {Message}", ex.Message);
                }
            }
        });
        builder.ConfigureTestServices(services =>
        {
            // Add a fake authentication scheme for testing
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
    }
}
