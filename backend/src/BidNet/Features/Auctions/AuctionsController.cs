using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BidNet.Features.Auctions.Commands;
using BidNet.Features.Auctions.Queries;
using BidNet.Features.Auctions.Models;
using BidNet.Shared.Controllers;

namespace BidNet.Features.Auctions;

[Route("api/[controller]")]
public class AuctionsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AuctionsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize]
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
        return result.Match(
            auction => Ok(_mapper.Map<AuctionResponse>(auction)),
            HandleErrors);
    }

    [HttpPut("{id}")]
    [Authorize]
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
        return result.Match(
            auction => Ok(_mapper.Map<AuctionResponse>(auction)),
            HandleErrors);
    }

    [HttpDelete("{id}")]
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
        return result.Match(
            auction => Ok(_mapper.Map<AuctionResponse>(auction)),
            HandleErrors);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionsList()
    {
        var query = new GetAuctionsListQuery();
        var result = await _mediator.Send(query);
        return result.Match(
            auctions => Ok(_mapper.Map<IEnumerable<AuctionResponse>>(auctions)),
            HandleErrors);
    }
}
