namespace BidNet.Features.Auctions.Models;

public record AuctionDto(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    decimal StartingPrice,
    Guid CreatedBy);

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
