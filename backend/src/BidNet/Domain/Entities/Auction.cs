using Ardalis.GuardClauses;
using BidNet.Domain.Enums;
using ErrorOr;

namespace BidNet.Domain.Entities;

public readonly record struct AuctionId(Guid Value)
{
    public static implicit operator Guid(AuctionId id) => id.Value;
    public static implicit operator AuctionId(Guid id) => new(id);
}

public class Auction
{
    public AuctionId Id { get; init; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal StartingPrice { get; private set; }
    public decimal? CurrentPrice { get; private set; }

    public UserId CreatedBy { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    public AuctionStatus Status { get; private set; } = AuctionStatus.Scheduled;

    public UserId? WinnerId { get; private set; }
    public User? Winner { get; private set; }

    private List<Bid> _bids { get; set; } = [];
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

    // Start the auction
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
    }
}
