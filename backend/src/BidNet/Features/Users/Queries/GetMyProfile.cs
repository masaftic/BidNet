using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Users.Mapping;
using BidNet.Features.Users.Models;
using BidNet.Shared.Services;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BidNet.Features.Users.Queries;

public record GetMyProfileQuery : IRequest<ErrorOr<UserDto>>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ErrorOr<UserDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<UserRole> _roleManager;

    public GetMyProfileQueryHandler(
        AppDbContext dbContext, 
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        // Get the base user data
        var userId = _currentUserService.UserId;
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        
        if (user is null)
        {
            return Error.NotFound(description: "User not found");
        }
        
        // Create the DTO with basic information
        var userDto = user.ToUserDto();
        
        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        userDto.Roles = string.Join(", ", roles);
        
        // Get wallet information
        var wallet = await _dbContext.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == user.Id, cancellationToken);
            
        if (wallet != null)
        {
            userDto.Balance = wallet.Balance;
            userDto.HeldBalance = wallet.HeldBalance;
            userDto.Currency = wallet.Currency;
        }
        
        // Get activity statistics
        userDto.TotalAuctions = await _dbContext.Auctions
            .CountAsync(a => a.CreatedBy == user.Id, cancellationToken);
            
        userDto.ActiveAuctions = await _dbContext.Auctions
            .CountAsync(a => a.CreatedBy == user.Id && a.Status == AuctionStatus.Live, cancellationToken);
            
        userDto.TotalBids = await _dbContext.Bids
            .CountAsync(b => b.UserId == user.Id, cancellationToken);
            
        userDto.WonAuctions = await _dbContext.Auctions
            .CountAsync(a => a.WinnerId == user.Id, cancellationToken);

        return userDto;
    }
}
