using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BidNet.Features.Users.Identity;
using BidNet.Data.Persistence;
using BidNet.Features.Users.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BidNet.Features.Auth.Models;
using BidNet.Features.Auth.Abstractions;
using BidNet.Features.Bids.Services;
using BidNet.Shared.Services;

namespace BidNet;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // Add SignalR services
        services.AddSignalR();
        services.AddScoped<IBidNotificationService, BidNotificationService>();

        services.AddPersistence(configuration);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer((options) =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            // Configure SignalR authentication
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Get the token from the query string for SignalR connections
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Custom logic after token validation
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    // Custom logic on authentication failure
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();

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