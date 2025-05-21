using Bidnet.Domain.Entities;
using Bidnet.Domain.Enums;

namespace Bidnet.Api.Models;

public class CreateAuctionRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartingPrice { get; set; }
}

public class UpdateAuctionRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartingPrice { get; set; }
}

public class AuctionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public string Status { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public Guid? WinnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
