using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Models;
using BidNet.Features.Auctions.Services;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Auctions.Commands;

public class UploadAuctionImageCommandValidator : AbstractValidator<UploadAuctionImageCommand>
{
    public UploadAuctionImageCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty();
            
        RuleFor(x => x.ImageData)
            .NotEmpty()
            .WithMessage("Image data cannot be empty");
            
        RuleFor(x => x.FileName)
            .MaximumLength(100)
            .When(x => x.FileName != null);
            
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(x => x.StartsWith("image/"))
            .WithMessage("Only image files are allowed");
    }
}

public record UploadAuctionImageCommand(
    AuctionId AuctionId,
    byte[] ImageData,
    string? FileName,
    string ContentType,
    bool IsPrimary = false) : IRequest<ErrorOr<AuctionImageDto>>;

public class UploadAuctionImageCommandHandler : IRequestHandler<UploadAuctionImageCommand, ErrorOr<AuctionImageDto>>
{
    private readonly AuctionImageService _auctionImageService;
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UploadAuctionImageCommandHandler(AuctionImageService auctionImageService, ICurrentUserService currentUser, AppDbContext db)
    {
        _auctionImageService = auctionImageService;
        _currentUser = currentUser;
        _db = db;
    }

    public async Task<ErrorOr<AuctionImageDto>> Handle(UploadAuctionImageCommand request, CancellationToken cancellationToken)
    {
        if (await _db.Auctions
            .Where(x => x.Id == request.AuctionId)
            .AllAsync(x => x.CreatedBy != _currentUser.UserId, cancellationToken: cancellationToken))
        {
            return Error.Forbidden(description: "You do not have permission to upload images for this auction.");
        }

        using var memoryStream = new MemoryStream(request.ImageData);

        var res = await _auctionImageService.AddImageAsync(
            request.AuctionId,
            memoryStream,
            request.FileName,
            request.ContentType,
            request.IsPrimary,
            cancellationToken);

        if (res.IsError) return res.Errors;

        var img = res.Value;
        return new AuctionImageDto(img.Id, img.AuctionId, img.ImageUrl, img.IsPrimary);
    }
}
