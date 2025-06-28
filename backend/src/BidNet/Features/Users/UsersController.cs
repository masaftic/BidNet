using BidNet.Features.Users.Commands;
using BidNet.Features.Users.Models;
using BidNet.Features.Users.Queries;
using BidNet.Shared.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidNet.Features.Users;

[Route("api/[controller]")]
public class UsersController(IMediator mediator) : ApiController
{
    [HttpGet("me/profile")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var query = new GetMyProfileQuery();
        var result = await mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpPut("me/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var command = new UpdateMyProfileCommand(
            request.UserName,
            request.Email,
            request.CurrentPassword,
            request.NewPassword);

        var result = await mediator.Send(command);
        return result.Match(Ok, HandleErrors);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListUsers()
    {
        var query = new ListUsersQuery();
        var result = await mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await mediator.Send(query);
        return result.Match(Ok, HandleErrors);
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        var command = new DeleteUserCommand(userId);
        var result = await mediator.Send(command);
        return result.Match(_ => NoContent(), HandleErrors);
    }

    [HttpPut("me/request-seller-role")]
    [Authorize]
    public async Task<IActionResult> RequestSellerRoleCommand()
    {
        var command = new RequestSellerRoleCommand();
        var result = await mediator.Send(command);
        return result.Match(_ => NoContent(), HandleErrors);
    }
}
