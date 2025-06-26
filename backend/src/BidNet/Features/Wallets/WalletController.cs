using BidNet.Features.Wallets.Commands;
using BidNet.Features.Wallets.Models;
using BidNet.Features.Wallets.Queries;
using BidNet.Shared.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Wallets;

[Route("api/[controller]")]
[Authorize]
public class WalletController : ApiController
{
    private readonly IMediator _mediator;

    public WalletController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetWalletBalance()
    {
        var query = new GetWalletBalanceQuery();
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetTransactionHistoryQuery(page, pageSize);
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> DepositFunds([FromBody] DepositRequest request)
    {
        var command = new DepositFundsCommand(request.Amount, request.PaymentMethod, request.PaymentDetails);
        var result = await _mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> WithdrawFunds([FromBody] WithdrawalRequest request)
    {
        var command = new WithdrawFundsCommand(request.Amount, request.WithdrawalMethod, request.WithdrawalDetails);
        var result = await _mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferFunds([FromBody] TransferFundsRequest request)
    {
        var command = new TransferFundsCommand(request.RecipientId, request.Amount, request.Description);
        var result = await _mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }
}
