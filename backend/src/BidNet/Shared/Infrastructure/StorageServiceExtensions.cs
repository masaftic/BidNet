using BidNet.Shared.Abstractions.Storage;
using BidNet.Shared.Infrastructure.Storage;

namespace BidNet.Shared.Infrastructure;

public static class StorageServiceExtensions
{
    public static IServiceCollection AddImageStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
        
        services.AddScoped<IImageStorage, FileSystemImageStorage>();
        
        return services;
    }
}
