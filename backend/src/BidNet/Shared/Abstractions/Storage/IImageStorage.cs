using ErrorOr;

namespace BidNet.Shared.Abstractions.Storage;

public interface IImageStorage
{
    Task<ErrorOr<string>> StoreImageAsync(byte[] imageData, string? fileName = null, string? contentType = null);
    Task<ErrorOr<string>> StoreImageAsync(Stream imageStream, string? fileName = null, string? contentType = null);
    Task<ErrorOr<Success>> DeleteImageAsync(string imageUrl);
    string GetImageUrl(string imagePath);
}
