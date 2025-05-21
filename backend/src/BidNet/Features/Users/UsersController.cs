using AutoMapper;
using BidNet.Features.Auth.Models;
using BidNet.Features.Users.Commands;
using BidNet.Features.Users.Models;
using BidNet.Features.Users.Queries;
using BidNet.Shared.Abstractions;
using BidNet.Shared.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Users;

[Route("api/[controller]")]
public class UsersController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(
        IMediator mediator, 
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    [HttpGet("me/profile")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var query = new GetMyProfileQuery();
        var result = await _mediator.Send(query);
        return result.Match(
            profile => Ok(_mapper.Map<UserProfileResponse>(profile)),
            HandleErrors);
    }

    [HttpPut("me/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var command = new UpdateMyProfileCommand(
            request.Username,
            request.Email,
            request.CurrentPassword,
            request.CurrentPassword);
            
        var result = await _mediator.Send(command);
        return result.Match(
            profile => Ok(_mapper.Map<UserProfileResponse>(profile)),
            HandleErrors);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListUsers()
    {
        var query = new ListUsersQuery();
        var result = await _mediator.Send(query);
        return result.Match(
            users => Ok(_mapper.Map<List<UserResponse>>(users)),
            HandleErrors);
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await _mediator.Send(query);
        return result.Match(
            user => Ok(_mapper.Map<UserResponse>(user)),
            HandleErrors);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var command = new DeleteUserCommand(userId);
        var result = await _mediator.Send(command);
        return result.Match(
            _ => NoContent(),
            HandleErrors);
    }
}
