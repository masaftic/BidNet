using BidNet.Shared.Behaviors;
using FluentValidation;
using MediatR;

namespace BidNet.Shared;

public static class SharedLayer
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(SharedLayer).Assembly);

            cfg.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(SharedLayer).Assembly);

        return services;
    }
}
