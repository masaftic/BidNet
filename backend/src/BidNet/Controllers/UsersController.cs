using AutoMapper;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Users.Commands;
using BidNet.Features.Users.Queries;
using BidNet.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Controllers;

[Route("api/[controller]")]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UsersController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
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
            authResult => Ok(_mapper.Map<AuthenticationResponse>(authResult)),
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
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command);
        return result.Match(
            authResult => Ok(_mapper.Map<AuthenticationResponse>(authResult)),
            HandleErrors);
    }
}
