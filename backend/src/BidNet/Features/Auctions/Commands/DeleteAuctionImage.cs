using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Services;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Commands;

public class DeleteAuctionImageCommandValidator : AbstractValidator<DeleteAuctionImageCommand>
{
    public DeleteAuctionImageCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty();

        RuleFor(x => x.ImageId)
            .NotEmpty();
    }
}

public record DeleteAuctionImageCommand(
    AuctionId AuctionId,
    AuctionImageId ImageId) : IRequest<ErrorOr<Success>>;

public class DeleteAuctionImageCommandHandler : IRequestHandler<DeleteAuctionImageCommand, ErrorOr<Success>>
{
    private readonly AuctionImageService _auctionImageService;
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteAuctionImageCommandHandler(AuctionImageService auctionImageService, AppDbContext db, ICurrentUserService userService)
    {
        _auctionImageService = auctionImageService;
        _db = db;
        _currentUser = userService;
    }

    public async Task<ErrorOr<Success>> Handle(DeleteAuctionImageCommand request, CancellationToken cancellationToken)
    {
        if (await _db.Auctions
            .Where(x => x.Id == request.AuctionId)
            .AllAsync(x => x.CreatedBy != _currentUser.UserId, cancellationToken: cancellationToken))
        {
            return Error.Forbidden(description: "You do not have permission to delete images for this auction.");
        }

        return await _auctionImageService.DeleteImageAsync(
            request.AuctionId,
            request.ImageId,
            cancellationToken);
    }
}
