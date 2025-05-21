using AutoMapper;
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
    private readonly IMapper _mapper;

    public BidsController(
        IMediator mediator, 
        IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PlaceBid(PlaceBidRequest request)
    {
        var command = new PlaceBidCommand(request.AuctionId, request.Amount);
        var result = await _mediator.Send(command);
        return result.Match(
            bid => Ok(_mapper.Map<BidResponse>(bid)),
            HandleErrors);
    }

    [HttpGet("auctions/{auctionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionBids(Guid auctionId)
    {
        var query = new GetAuctionBidsQuery(auctionId);
        var result = await _mediator.Send(query);
        return result.Match(
            bidList => Ok(bidList),
            HandleErrors);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyBids()
    {
        var query = new GetMyBidsQuery();
        var result = await _mediator.Send(query);
        return result.Match(
            bidList => Ok(bidList),
            HandleErrors);
    }
}
