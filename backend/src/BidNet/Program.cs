using BidNet;
using BidNet.Data.Persistence;
using BidNet.Features.Auctions.Services;
using BidNet.Features.Bids.Hubs;
using BidNet.Features.Wallets.Hubs;
using BidNet.Shared;
using ErrorOr;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureInfrastructure(builder.Services, builder.Configuration);
ConfigureSwagger(builder.Services);
ConfigureCors(builder.Services);
ConfigureProblemDetails(builder.Services);
ConfigureControllers(builder.Services);

// Configure application services
builder.Services.AddSharedServices();

// Create and configure application
var app = builder.Build();

// Seed data
await SeedDataAsync(app);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    ConfigureDevelopmentEnvironment(app);
}

ConfigureMiddleware(app);

// Configure Hangfire Dashboard 
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    
});

ConfigureEndpoints(app);

// Start the auction scheduler
using (var scope = app.Services.CreateScope())
{
    var auctionScheduler = scope.ServiceProvider.GetRequiredService<AuctionSchedulerService>();
    auctionScheduler.ScheduleAllPendingAuctions();
    Console.WriteLine("Auction scheduler started");
}

app.Run();

#region Service Configuration Methods

static void ConfigureInfrastructure(IServiceCollection services, IConfiguration configuration)
{
    services.AddInfrastructure(configuration);
}

static void ConfigureSwagger(IServiceCollection services)
{
    services.AddOpenApi();
    services.AddSwaggerGen(options =>
    {
        // Add JWT Authentication support to Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

static void ConfigureProblemDetails(IServiceCollection services)
{
    services.AddProblemDetails(o =>
    {
        o.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance =
                $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            
            context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.Request.Headers.RequestId.ToString());

            if (context.HttpContext.Items["errors"] is List<Error> errors)
            {
                context.ProblemDetails.Extensions.TryAdd("errorsCodes", errors.Select(e => e.Code).ToArray());
            }
        };
    });
}

static void ConfigureCors(IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowed(_ => true) // For development; in production, restrict this
                  .AllowCredentials();
        });
    });
}

static void ConfigureControllers(IServiceCollection services)
{
    services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
}

#endregion

#region Application Configuration Methods

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAsync();
}

static void ConfigureDevelopmentEnvironment(WebApplication app)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BidNet API V1");
        options.RoutePrefix = string.Empty; // Serve Swagger at the app's root
        options.EnablePersistAuthorization(); // Persist authorization data between refreshes
        options.DisplayRequestDuration(); // Show how long requests take
        options.DocExpansion(DocExpansion.None); // Collapse documentation by default
    });
}

static void ConfigureMiddleware(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // Request logging middleware
    app.Use(async (ctx, next) =>
    {
        logger.LogInformation("Handling Request {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        await next();
    });
    
    app.UseHttpsRedirection();
    
    // Static files for images
    app.UseStaticFiles();
    
    // Exception handling
    app.UseExceptionHandler(o =>
    {
        o.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            if (exception != null)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    Error = "An unexpected error occurred.",
                    Details = exception.Message
                });
            }
        });
    });
    
    app.UseRouting();
    
    // CORS middleware
    app.UseCors("CorsPolicy");
    
    // Authentication middleware
    app.UseAuthentication();
    app.UseAuthorization();
}

static void ConfigureEndpoints(WebApplication app)
{
    // Map SignalR hubs
    app.MapHub<BidHub>("/hubs/bid");
    app.MapHub<WalletHub>("/hubs/wallet");
    
    // Map controllers
    app.MapControllers();
}

#endregion

// This partial class allows for test projects to reference this Program class
public partial class Program
{
}