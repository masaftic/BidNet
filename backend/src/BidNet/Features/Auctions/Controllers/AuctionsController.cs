using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Commands;
using BidNet.Features.Auctions.Models;
using BidNet.Features.Auctions.Queries;
using BidNet.Shared.Controllers;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Auctions.Controllers;

[Route("api/auctions")]
public class AuctionsController : ApiController
{
    private readonly IMediator _mediator;
    
    public AuctionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionCommand command)
    {
        var result = await _mediator.Send(command);
        
        return result.Match(
            auction => CreatedAtAction(
                nameof(GetAuctionById),
                new { id = auction.Id },
                auction),
            errors => HandleErrors(errors));
    }
    
    [HttpPost("with-images")]
    public async Task<IActionResult> CreateAuctionWithImages([FromForm] CreateAuctionWithImagesRequest request)
    {
        // Convert the form model to command
        var command = new CreateAuctionCommand
        {
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StartingPrice = request.StartingPrice,
            Images = new List<AuctionImageInfo>()
        };
        
        // Process images if any
        if (request.Images != null && request.Images.Count > 0)
        {
            bool hasPrimary = false;
            
            foreach (var image in request.Images)
            {
                using var memoryStream = new MemoryStream();
                await image.CopyToAsync(memoryStream);
                
                // If this is the first image and no primary is set, make it primary
                bool isPrimary = request.PrimaryImageIndex.HasValue 
                    ? Array.IndexOf(request.Images.ToArray(), image) == request.PrimaryImageIndex.Value
                    : !hasPrimary;
                
                if (isPrimary)
                {
                    hasPrimary = true;
                }
                
                command.Images.Add(new AuctionImageInfo
                {
                    ImageData = memoryStream.ToArray(),
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    IsPrimary = isPrimary
                });
            }
        }
        
        var result = await _mediator.Send(command);
        
        return result.Match(
            auction => CreatedAtAction(
                nameof(GetAuctionById),
                new { id = auction.Id },
                auction),
            errors => HandleErrors(errors));
    }
    
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Seller, Admin")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionRequest request)
    {
        var command = new UpdateAuctionCommand(
            id,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.StartingPrice
        );

        var result = await _mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller, Admin")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var command = new DeleteAuctionCommand(id);
        var result = await _mediator.Send(command);
        return result.Match(_ => NoContent(), HandleErrors);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        var query = new GetAuctionByIdQuery(id);
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionsList(int pageIndex = 0, int pageSize = 10)
    {
        var query = new GetAuctionsListQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }
}

