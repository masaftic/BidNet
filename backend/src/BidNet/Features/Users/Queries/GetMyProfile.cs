using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Auth.Models;
using BidNet.Features.Users.Mapping;
using BidNet.Shared.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Queries;

public record GetMyProfileQuery : IRequest<ErrorOr<UserDto>>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, ErrorOr<UserDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileQueryHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .ToUserDto()
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user is null)
        {
            return Error.NotFound(description: "User not found");
        }

        return user;
    }
}
