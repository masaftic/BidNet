using Ardalis.GuardClauses;
using Bidnet.Domain.Enums;

namespace Bidnet.Domain.Entities;

public readonly record struct AuctionId(Guid Value)
{
    public static implicit operator Guid(AuctionId id) => id.Value;
    public static implicit operator AuctionId(Guid id) => new(id);
}

public class Auction
{
    public AuctionId Id { get; set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal StartingPrice { get; private set; }
    public decimal? CurrentPrice { get; private set; }
    public UserId CreatedBy { get; private set; }
    public AuctionStatus Status { get; private set; } = AuctionStatus.Scheduled;
    public UserId? WinnerId { get; private set; }
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
}
