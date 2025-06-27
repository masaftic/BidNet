namespace BidNet.Shared.Infrastructure.Storage;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";
    
    public string BasePath { get; set; } = "wwwroot/images/auctions";
    public string BaseUrl { get; set; } = "/images/auctions";
    public int MaxFileSizeMb { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif"];
}
