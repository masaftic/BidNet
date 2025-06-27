using Ardalis.GuardClauses;
using BidNet.Domain.Enums;
using ErrorOr;

namespace BidNet.Domain.Entities;

public readonly record struct AuctionId(Guid Value)
{
    public static implicit operator Guid(AuctionId id) => id.Value;
    public static implicit operator AuctionId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class Auction
{
    public AuctionId Id { get; init; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal StartingPrice { get; private set; }    public decimal? CurrentPrice { get; private set; }
    
    private List<AuctionImage> _images = [];
    public IReadOnlyCollection<AuctionImage> Images => _images.AsReadOnly();

    public UserId CreatedBy { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    public AuctionStatus Status { get; private set; } = AuctionStatus.Scheduled;

    public UserId? WinnerId { get; private set; }
    public User? Winner { get; private set; }

    private List<Bid> _bids = [];
    public IReadOnlyList<Bid> Bids => _bids.AsReadOnly();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Auction() { }

    public Auction(string title, string description, DateTime startDate, DateTime endDate, decimal startingPrice, UserId createdBy)
    {
        Guard.Against.NullOrEmpty(title, nameof(title));
        Guard.Against.NullOrEmpty(description, nameof(description));
        Guard.Against.OutOfSQLDateRange(startDate, nameof(startDate));
        Guard.Against.OutOfSQLDateRange(endDate, nameof(endDate));
        Guard.Against.NegativeOrZero(startingPrice, nameof(startingPrice));

        if (endDate <= startDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
        }

        if (startingPrice <= 0)
        {
            throw new ArgumentException("Starting price must be greater than zero.", nameof(startingPrice));
        }

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        StartingPrice = startingPrice;
        CreatedBy = createdBy;
    }

    public void UpdateDetails(string title, string description, DateTime startDate, DateTime endDate, decimal startingPrice)
    {
        Guard.Against.NullOrEmpty(title, nameof(title));
        Guard.Against.NullOrEmpty(description, nameof(description));
        Guard.Against.OutOfSQLDateRange(startDate, nameof(startDate));
        Guard.Against.OutOfSQLDateRange(endDate, nameof(endDate));
        Guard.Against.NegativeOrZero(startingPrice, nameof(startingPrice));

        if (endDate <= startDate)
        {
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
        }

        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        StartingPrice = startingPrice;
    }

    public void UpdateCurrentPrice(decimal? currentPrice)
    {
        if (currentPrice is <= 0)
        {
            throw new ArgumentException("Current price must be greater than zero.", nameof(currentPrice));
        }

        CurrentPrice = currentPrice;
    }


    /// <summary>
    /// Starts the auction
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Start()
    {
        if (Status != AuctionStatus.Scheduled)
        {
            throw new InvalidOperationException("Auction can only be started from the scheduled state.");
        }

        Status = AuctionStatus.Live;
    }

    public ErrorOr<Success> PlaceBid(Bid bid)
    {
        if (Status != AuctionStatus.Live)
        {
            return Error.Validation(description: "Auction is not live");
        }

        if (CreatedBy == bid.UserId)
        {
            return Error.Validation(description: "You cannot bid on your own auction");
        }

        // Check if amount is higher than current price
        if (CurrentPrice.HasValue && bid.Amount <= CurrentPrice.Value)
        {
            return Error.Validation(description: "Bid amount must be higher than current price");
        }

        // Check if amount is higher than starting price
        if (!CurrentPrice.HasValue && bid.Amount <= StartingPrice)
        {
            return Error.Validation(description: "Bid amount must be higher than starting price");
        }

        var winningBid = _bids
            .Where(b => b.AuctionId == Id && b.IsWinning)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefault();

        // If there is a winning bid, update its status
        if (winningBid != null && winningBid.Amount >= bid.Amount)
        {
            return Error.Validation(description: "Bid amount must be higher than the current winning bid");
        }

        winningBid?.UpdateWinningStatus(false);
        bid.UpdateWinningStatus(true);
        UpdateCurrentPrice(bid.Amount);

        _bids.Add(bid);

        return Result.Success;
    }    // Methods for handling images
    public AuctionImage AddImage(string imageUrl, bool isPrimary = false)
    {
        Guard.Against.NullOrEmpty(imageUrl, nameof(imageUrl));
        
        // Check if we already have this image
        var existingImage = _images.FirstOrDefault(i => i.ImageUrl == imageUrl);
        if (existingImage != null)
        {
            return existingImage;
        }
        
        // Create new image
        var image = new AuctionImage(Id, imageUrl, isPrimary);
        
        // If this is the first image or marked as primary
        if (isPrimary || !_images.Any(i => i.IsPrimary))
        {
            // Reset any existing primary image
            foreach (var img in _images.Where(i => i.IsPrimary))
            {
                img.SetAsPrimary(false);
            }
            
            image.SetAsPrimary(true);
        }
        
        _images.Add(image);
        return image;
    }
    
    public bool RemoveImage(string imageUrl)
    {
        var image = _images.FirstOrDefault(i => i.ImageUrl == imageUrl);
        if (image == null) return false;
        
        bool wasPrimary = image.IsPrimary;
        bool result = _images.Remove(image);
        
        // If we removed the primary image, set a new one if available
        if (result && wasPrimary && _images.Any())
        {
            _images.First().SetAsPrimary(true);
        }
        
        return result;
    }
    
    public void SetPrimaryImage(string imageUrl)
    {
        Guard.Against.NullOrEmpty(imageUrl, nameof(imageUrl));
        
        var image = _images.FirstOrDefault(i => i.ImageUrl == imageUrl);
        if (image == null)
        {
            throw new ArgumentException("Image not found in this auction's images.", nameof(imageUrl));
        }
        
        // Reset existing primary images
        foreach (var img in _images.Where(i => i.IsPrimary))
        {
            img.SetAsPrimary(false);
        }
        
        // Set the new primary image
        image.SetAsPrimary(true);
    }
    
    public AuctionImage? GetPrimaryImage() => _images.FirstOrDefault(i => i.IsPrimary);
}
