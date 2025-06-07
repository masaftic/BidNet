using BidNet.Features.Bids.Commands;
using BidNet.Features.Bids.Models;
using BidNet.Features.Bids.Queries;
using BidNet.Shared.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Bids;

[Route("api/[controller]")]
public class BidsController : ApiController
{
    private readonly IMediator _mediator;

    public BidsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PlaceBid(PlaceBidRequest request)
    {
        var command = new PlaceBidCommand(request.AuctionId, request.Amount);
        var result = await _mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }

    [HttpGet("auctions/{auctionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionBids(Guid auctionId)
    {
        var query = new GetAuctionBidsQuery(auctionId);
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyBids()
    {
        var query = new GetMyBidsQuery();
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }
}
