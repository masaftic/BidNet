using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Shared.Abstractions.Storage;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Services;

public class AuctionImageService
{
    private readonly AppDbContext _dbContext;
    private readonly IImageStorage _imageStorage;

    public AuctionImageService(
        AppDbContext dbContext,
        IImageStorage imageStorage)
    {
        _dbContext = dbContext;
        _imageStorage = imageStorage;
    }

    public async Task<ErrorOr<AuctionImage>> AddImageAsync(
        AuctionId auctionId,
        Stream imageStream,
        string? fileName = null,
        string? contentType = null,
        bool isPrimary = false,
        CancellationToken cancellationToken = default)
    {
        var auction = await _dbContext.Auctions
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == auctionId, cancellationToken);
        
        if (auction == null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }

        // Upload the image to storage
        var storeResult = await _imageStorage.StoreImageAsync(imageStream, fileName, contentType);
        if (storeResult.IsError)
        {
            return storeResult.Errors;
        }

        // Add the image to the auction
        var auctionImage = auction.AddImage(storeResult.Value, isPrimary);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return auctionImage;
    }
    
    public async Task<ErrorOr<Success>> DeleteImageAsync(
        AuctionId auctionId,
        AuctionImageId imageId,
        CancellationToken cancellationToken = default)
    {
        var image = await _dbContext.AuctionImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.AuctionId == auctionId, cancellationToken);
            
        if (image == null)
        {
            return Error.NotFound("AuctionImage.NotFound", "Image not found for this auction");
        }
        
        // Delete from storage
        var deleteResult = await _imageStorage.DeleteImageAsync(image.ImageUrl);
        if (deleteResult.IsError)
        {
            return deleteResult.Errors;
        }
        
        // Remove from database
        _dbContext.AuctionImages.Remove(image);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
    
    public async Task<ErrorOr<Success>> SetPrimaryImageAsync(
        AuctionId auctionId,
        AuctionImageId imageId,
        CancellationToken cancellationToken = default)
    {
        var auction = await _dbContext.Auctions
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == auctionId, cancellationToken);
            
        if (auction == null)
        {
            return Error.NotFound("Auction.NotFound", "Auction not found");
        }
        
        var image = auction.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return Error.NotFound("AuctionImage.NotFound", "Image not found for this auction");
        }
        
        // Reset all images
        foreach (var img in auction.Images.Where(i => i.IsPrimary))
        {
            img.SetAsPrimary(false);
        }
        
        // Set the new primary
        image.SetAsPrimary(true);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}
