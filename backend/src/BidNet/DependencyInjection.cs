using Microsoft.EntityFrameworkCore;
using BidNet.Data.Persistence;
using BidNet.Features.Bids.Services;
using BidNet.Shared.Services;
using BidNet.Domain.Entities;
using BidNet.Features.Authentication.Models;
using BidNet.Features.Authentication.Services;
using BidNet.Features.Auctions.Services;
using BidNet.Features.Wallets.Services;
using BidNet.Shared.Abstractions.Storage;
using BidNet.Shared.Infrastructure.Storage;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BidNet;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();        // Add SignalR services
        services.AddSignalR();
        services.AddScoped<IBidNotificationService, BidNotificationService>();
        services.AddScoped<IWalletNotificationService, WalletNotificationService>();
        
        // Add image storage
        services.AddScoped<IImageStorage, FileSystemImageStorage>();
        
        // Add Hangfire services
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));
            
        // Add the processing server as IHostedService
        services.AddHangfireServer();
        
        // Register the auction services
        services.AddScoped<AuctionSchedulerService>();
        services.AddScoped<AuctionImageService>();
        
        services.AddPersistence(configuration);
        services.AddIdentityCore<User>(options =>
            { 
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<UserRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<User>>()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<UserRole>>()
            .AddDefaultTokenProviders();        
        
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var jwtSettings = services.BuildServiceProvider().GetService<IOptions<JwtSettings>>()!;
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = jwtSettings.Value.Issuer,
                    ValidAudience = jwtSettings.Value.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key))
                };
            });

        services.AddAuthorization();
        
        // Register authentication services
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }


    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<DataSeeder>();

        return services;
    }
}