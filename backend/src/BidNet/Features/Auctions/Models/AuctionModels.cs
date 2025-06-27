using Microsoft.AspNetCore.Http;

namespace BidNet.Features.Auctions.Models;

public record AuctionDto(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal StartingPrice,
    decimal? CurrentPrice,
    Guid CreatedBy,
    List<AuctionImageDto> Images);

public record AuctionImageDto(
    Guid Id,
    Guid AuctionId,
    string ImageUrl,
    bool IsPrimary);

public record CreateAuctionRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal StartingPrice);

public record UpdateAuctionRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal StartingPrice);

public class CreateAuctionWithImagesRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartingPrice { get; set; }
    public List<IFormFile>? Images { get; set; }
    public int? PrimaryImageIndex { get; set; }
}
