using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Shared.Services;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Commands;

public record RequestSellerRoleCommand : IRequest<ErrorOr<Success>>;

public class RequestSellerRoleCommandHandler : IRequestHandler<RequestSellerRoleCommand, ErrorOr<Success>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly UserManager<User> _userManager;

    public RequestSellerRoleCommandHandler(ICurrentUserService currentUser, UserManager<User> userManager)
    {
        _currentUser = currentUser;
        _userManager = userManager;
    }

    public async Task<ErrorOr<Success>> Handle(RequestSellerRoleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user == null)
        {
            return Error.NotFound(description: "User not found.");
        }

        await _userManager.AddToRoleAsync(user, UserRoles.Seller);
        return Result.Success;
    }
}