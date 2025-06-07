using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BidNet.Features.Auctions.Commands;
using BidNet.Features.Auctions.Queries;
using BidNet.Features.Auctions.Models;
using BidNet.Shared.Controllers;

namespace BidNet.Features.Auctions;

[Route("api/[controller]")]
public class AuctionsController : ApiController
{
    private readonly IMediator _mediator;

    public AuctionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Seller, Admin")]
    public async Task<IActionResult> CreateAuction(CreateAuctionRequest request)
    {
        var command = new CreateAuctionCommand(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.StartingPrice
        );

        var result = await _mediator.Send(command);

        return result.Match((res) => CreatedAtAction(nameof(GetAuctionById), new { id = res.Id }, res), HandleErrors);
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
    public async Task<IActionResult> GetAuctionsList()
    {
        var query = new GetAuctionsListQuery();
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }
}
