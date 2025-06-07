using System.Net;
using System.Net.Http.Json;
using BidNet.Features.Auctions.Models;
using FluentAssertions;

namespace Bidnet.Api.Tests.EndToEnd;

public class AuctionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuctionTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAuction_ShouldReturnCreatedAuction()
    {
        // Arrange
        var request = new CreateAuctionRequest
        (
            Title: "Test Auction",
            Description: "Test Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(10),
            StartingPrice: 100.0m
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auctions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        auction.Should().NotBeNull();
        auction!.Title.Should().Be(request.Title);
        auction.Description.Should().Be(request.Description);
        auction.StartingPrice.Should().Be(request.StartingPrice);
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnAuction()
    {
        // Arrange
        var auctionId = await SeedAuctionAsync(); // Ensure a valid auction ID is used

        // Act
        var response = await _client.GetAsync($"/api/auctions/{auctionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        auction.Should().NotBeNull();
        auction.Id.Should().Be(auctionId);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnUpdatedAuction()
    {
        // Arrange
        var auctionId = await SeedAuctionAsync(); // Ensure a valid auction ID is used
        var request = new UpdateAuctionRequest
        (
            Title: "Updated Title",
            Description: "Updated Description",
            StartDate: DateTime.UtcNow.AddDays(2),
            EndDate: DateTime.UtcNow.AddDays(12),
            StartingPrice: 200.0m
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/auctions/{auctionId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        auction.Should().NotBeNull();
        auction.Title.Should().Be(request.Title);
        auction.Description.Should().Be(request.Description);
        auction.StartingPrice.Should().Be(request.StartingPrice);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturnNoContent()
    {
        // Arrange
        var auctionId = await SeedAuctionAsync(); // Ensure a valid auction ID is used

        // Act
        var response = await _client.DeleteAsync($"/api/auctions/{auctionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAuctionsList_ShouldReturnAuctions()
    {
        // Act
        var response = await _client.GetAsync("/api/auctions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auctions = await response.Content.ReadFromJsonAsync<AuctionDto[]>();
        auctions.Should().NotBeNull();
        auctions.Should().NotBeEmpty();
    }

    private async Task<Guid> SeedAuctionAsync()
    {
        var request = new CreateAuctionRequest
        (
            Title: "Seeded Auction",
            Description: "Seeded Description",
            StartDate: DateTime.UtcNow.AddDays(1),
            EndDate: DateTime.UtcNow.AddDays(10),
            StartingPrice: 100.0m
        );

        var response = await _client.PostAsJsonAsync("/api/auctions", request);
        response.EnsureSuccessStatusCode();

        var auction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        return auction!.Id;
    }
}
