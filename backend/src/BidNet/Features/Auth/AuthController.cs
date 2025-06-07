using BidNet.Features.Auth.Commands;
using BidNet.Features.Auth.Models;
using BidNet.Shared.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Auth;

[Route("api/[controller]")]
public class AuthController : ApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.Role);
        var result = await _mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);
        return result.Match(
            authResult => Ok(MapAuthResult(authResult)),
            HandleErrors);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);
        return result.Match(
            authResult => Ok(new RefreshTokenResponse()
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            }),
            HandleErrors);
    }

    private LoginResponse MapAuthResult(AuthenticationResult authResult)
    {
        return new LoginResponse
        {
            Token = authResult.Token,
            RefreshToken = authResult.RefreshToken
        };
    }
}
