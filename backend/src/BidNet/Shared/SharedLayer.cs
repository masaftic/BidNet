using AutoMapper;
using BidNet.Features.Common.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BidNet.Shared;

public static class SharedLayer
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(SharedLayer).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(SharedLayer).Assembly);
        services.AddAutoMapper(typeof(SharedLayer).Assembly);

        return services;
    }
}
