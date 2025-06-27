using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Services;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Commands;

public class SetPrimaryAuctionImageCommandValidator : AbstractValidator<SetPrimaryAuctionImageCommand>
{
    public SetPrimaryAuctionImageCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty();
            
        RuleFor(x => x.ImageId)
            .NotEmpty();
    }
}

public record SetPrimaryAuctionImageCommand(
    AuctionId AuctionId,
    AuctionImageId ImageId) : IRequest<ErrorOr<Success>>;

public class SetPrimaryAuctionImageCommandHandler : IRequestHandler<SetPrimaryAuctionImageCommand, ErrorOr<Success>>
{
    private readonly AuctionImageService _auctionImageService;
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SetPrimaryAuctionImageCommandHandler(AuctionImageService auctionImageService, AppDbContext db, ICurrentUserService currentUser)
    {
        _auctionImageService = auctionImageService;
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ErrorOr<Success>> Handle(SetPrimaryAuctionImageCommand request, CancellationToken cancellationToken)
    {
        if (await _db.Auctions
            .Where(x => x.Id == request.AuctionId)
            .AllAsync(x => x.CreatedBy != _currentUser.UserId, cancellationToken: cancellationToken))
        {
            return Error.Forbidden(description: "You do not have permission to set primary images for this auction.");
        }

        return await _auctionImageService.SetPrimaryImageAsync(
            request.AuctionId,
            request.ImageId,
            cancellationToken);
    }
}
