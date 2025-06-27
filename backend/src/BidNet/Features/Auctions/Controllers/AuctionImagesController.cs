using BidNet.Features.Auctions.Commands;
using BidNet.Shared.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Auctions.Controllers;

[Route("api/auctions/{auctionId}/images")]
public class AuctionImagesController : ApiController
{
    private readonly IMediator _mediator;
    
    public AuctionImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadImage(
        [FromRoute] Guid auctionId, 
        IFormFile image,
        [FromForm] bool isPrimary = false)
    {
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);
        
        var command = new UploadAuctionImageCommand(
            auctionId,
            memoryStream.ToArray(), 
            image.FileName, 
            image.ContentType, 
            isPrimary);
            
        var result = await _mediator.Send(command);
        
        return result.Match(
            success => Ok(success),
            errors => HandleErrors(errors));
    }
    
    [HttpDelete("{imageId}")]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid auctionId, [FromRoute] Guid imageId)
    {
        var command = new DeleteAuctionImageCommand(auctionId, imageId);
        var result = await _mediator.Send(command);
        
        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }
    
    [HttpPut("{imageId}/set-primary")]
    public async Task<IActionResult> SetPrimaryImage([FromRoute] Guid auctionId, [FromRoute] Guid imageId)
    {
        var command = new SetPrimaryAuctionImageCommand(auctionId, imageId);
        var result = await _mediator.Send(command);
        
        return result.Match(
            _ => NoContent(),
            errors => HandleErrors(errors));
    }
}
