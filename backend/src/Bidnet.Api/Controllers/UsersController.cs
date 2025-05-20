using Bidnet.Api.Models;
using Bidnet.Application.Users.Commands;
using Bidnet.Application.Users.Queries;
using Bidnet.Domain.Entities;
using Bidnet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bidnet.Api.Controllers;

[Route("api/[controller]")]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
        {
            return BadRequest("Admin role is not allowed for registration.");
        }
        
        var command = new RegisterCommand(request.Username, request.Email, request.Password, request.Role);

        var result = await _mediator.Send(command);
        return result.Match(
            user => Ok(new UserResponse
            {
                Id = user.Id.Value,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            }),
            HandleErrors);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);
        return result.Match(
            authResult => Ok(new AuthenticationResponse
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            }),
            HandleErrors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery(new UserId(id));
        var result = await _mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.Token, request.RefreshToken);
        var result = await _mediator.Send(command);
        return result.Match(
            authResult => Ok(new AuthenticationResponse
            {
                Token = authResult.Token,
                RefreshToken = authResult.RefreshToken
            }),
            HandleErrors);
    }
}
