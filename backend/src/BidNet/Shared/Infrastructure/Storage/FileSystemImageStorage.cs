using System.Net;
using BidNet.Shared.Abstractions.Storage;
using ErrorOr;

namespace BidNet.Shared.Infrastructure.Storage;

public class FileSystemImageStorage : IImageStorage
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public FileSystemImageStorage(
        IConfiguration configuration, 
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        
        // Get configuration or use defaults
        _basePath = configuration.GetValue<string>("ImageStorage:BasePath") ?? "wwwroot/images/auctions";
        _baseUrl = configuration.GetValue<string>("ImageStorage:BaseUrl") ?? "/images/auctions";
        
        // Ensure the directory exists
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        var fullPath = GetFullDirectoryPath();
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }
    
    private string GetFullDirectoryPath()
    {
        // If the base path is relative, combine it with the web root path
        if (!Path.IsPathRooted(_basePath))
        {
            return Path.Combine(_environment.ContentRootPath, _basePath);
        }
        return _basePath;
    }
    
    public async Task<ErrorOr<string>> StoreImageAsync(byte[] imageData, string? fileName = null, string? contentType = null)
    {
        try
        {
            string uniqueFileName = GetUniqueFileName(fileName);
            string filePath = Path.Combine(GetFullDirectoryPath(), uniqueFileName);
            
            await File.WriteAllBytesAsync(filePath, imageData);
            
            return Path.Combine(_baseUrl, uniqueFileName).Replace('\\', '/');
        }
        catch (Exception ex)
        {
            return Error.Failure("ImageStorage.SaveFailed", ex.Message);
        }
    }

    public async Task<ErrorOr<string>> StoreImageAsync(Stream imageStream, string? fileName = null, string? contentType = null)
    {
        try
        {
            string uniqueFileName = GetUniqueFileName(fileName);
            string filePath = Path.Combine(GetFullDirectoryPath(), uniqueFileName);
            
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageStream.CopyToAsync(fileStream);
            
            return Path.Combine(_baseUrl, uniqueFileName).Replace('\\', '/');
        }
        catch (Exception ex)
        {
            return Error.Failure("ImageStorage.SaveFailed", ex.Message);
        }
    }

    public Task<ErrorOr<Success>> DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Extract filename from URL
            string fileName = Path.GetFileName(imageUrl);
            string filePath = Path.Combine(GetFullDirectoryPath(), fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult<ErrorOr<Success>>(Result.Success);
            }
            
            return Task.FromResult<ErrorOr<Success>>(Error.NotFound("ImageStorage.FileNotFound", "Image file not found"));
        }
        catch (Exception ex)
        {
            return Task.FromResult<ErrorOr<Success>>(Error.Failure("ImageStorage.DeleteFailed", ex.Message));
        }
    }

    public string GetImageUrl(string imagePath)
    {
        if (Uri.TryCreate(imagePath, UriKind.Absolute, out _))
        {
            // It's already an absolute URL
            return imagePath;
        }
        
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
        {
            // If no HttpContext is available, just return the relative path
            return imagePath;
        }
        
        // Construct an absolute URL
        var baseUrl = $"{request.Scheme}://{request.Host}";
        
        // If imagePath already starts with the base URL, return it as is
        if (imagePath.StartsWith(_baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return $"{baseUrl}{imagePath}";
        }
        
        // Otherwise, ensure proper path formatting
        if (!imagePath.StartsWith("/"))
        {
            imagePath = $"/{imagePath}";
        }
        
        return $"{baseUrl}{imagePath}";
    }
    
    private string GetUniqueFileName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            // Generate a unique name with a GUID
            return $"{Guid.NewGuid()}.jpg";
        }
        
        // Ensure fileName is safe to use as a filename
        string safeFileName = WebUtility.UrlEncode(Path.GetFileNameWithoutExtension(fileName));
        string extension = Path.GetExtension(fileName);
        
        // Add a timestamp to make it unique
        return $"{safeFileName}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
    }
}
