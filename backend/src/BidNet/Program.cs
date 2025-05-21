using BidNet;
using BidNet.Data.Persistence;
using BidNet.Features.Common.Behaviors;
using ErrorOr;
using FluentValidation;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = (context) =>
    {
        o.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance =
                $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

            if (context.HttpContext.Items["errors"] is List<Error> errors)
            {
                context.ProblemDetails.Extensions.TryAdd("errorsCodes", errors.Select(e => e.Code).ToArray());
            }
        };
    };
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }