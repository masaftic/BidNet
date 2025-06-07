using BidNet;
using BidNet.Data.Persistence;
using BidNet.Features.Bids.Hubs;
using ErrorOr;
using System.Text.Json.Serialization;
using BidNet.Shared;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        
        context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.Request.Headers["Request-Id"].ToString());

        if (context.HttpContext.Items["errors"] is List<Error> errors)
        {
            context.ProblemDetails.Extensions.TryAdd("errorsCodes", errors.Select(e => e.Code).ToArray());
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true) // For development; in production, restrict this
              .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSharedServices();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (ctx, next) =>
{
    Console.WriteLine(ctx.Request.Method);
    Console.WriteLine(ctx.Request.Path);
    await next();
});

app.UseHttpsRedirection();

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

// Add CORS middleware
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hubs
app.MapHub<BidHub>("/hubs/bid");

app.MapControllers();

app.Run();


public partial class Program
{
}