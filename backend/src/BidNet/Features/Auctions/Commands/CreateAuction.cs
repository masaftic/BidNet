using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Auctions.Models;
using BidNet.Features.Auctions.Services;
using BidNet.Shared.Abstractions.Storage;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BidNet.Features.Auctions.Commands;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0);
            
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .Must(date => date > DateTime.UtcNow)
            .WithMessage("Start date must be in the future");
            
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .Must((command, endDate) => endDate > command.StartDate)
            .WithMessage("End date must be after start date");
            
        // Optional images validation
        When(x => x.Images != null && x.Images.Count > 0, () => {
            RuleForEach(x => x.Images)
                .ChildRules(image => {
                    image.RuleFor(x => x.ImageData)
                        .NotEmpty()
                        .WithMessage("Image data cannot be empty");
                        
                    image.RuleFor(x => x.FileName)
                        .NotEmpty()
                        .MaximumLength(100);
                        
                    image.RuleFor(x => x.ContentType)
                        .NotEmpty()
                        .Must(x => x.StartsWith("image/"))
                        .WithMessage("Only image files are allowed");
                });
                
            // Validate that there's no more than one primary image
            RuleFor(x => x.Images)
                .Must(images => images != null && images.Count(img => img.IsPrimary) <= 1)
                .WithMessage("Only one image can be set as primary");
        });
    }
}

public class CreateAuctionCommand : IRequest<ErrorOr<AuctionDto>>
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartingPrice { get; set; }
    public List<AuctionImageInfo>? Images { get; set; }
}

public class AuctionImageInfo
{
    public byte[] ImageData { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, ErrorOr<AuctionDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _userService;
    private readonly AuctionSchedulerService _schedulerService;
    private readonly AuctionImageService _imageService;
    private readonly IImageStorage _imageStorage;

    public CreateAuctionCommandHandler(
        AppDbContext dbContext, 
        ICurrentUserService userService, 
        AuctionSchedulerService schedulerService,
        AuctionImageService imageService,
        IImageStorage imageStorage)
    {
        _dbContext = dbContext;
        _userService = userService;
        _schedulerService = schedulerService;
        _imageService = imageService;
        _imageStorage = imageStorage;
    }

    public async Task<ErrorOr<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = new Auction(request.Title, request.Description, request.StartDate, request.EndDate, request.StartingPrice, _userService.UserId);

        _dbContext.Auctions.Add(auction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        List<AuctionImage> images = [];

        // Handle images if any
        if (request.Images != null && request.Images.Count > 0)
        {
            foreach (var imageDto in request.Images)
            {
                using var imageStream = new MemoryStream(imageDto.ImageData);
                var result = await _imageService.AddImageAsync(
                    auction.Id,
                    imageStream,
                    imageDto.FileName,
                    imageDto.ContentType,
                    imageDto.IsPrimary,
                    cancellationToken);

                if (result.IsError) return result.Errors;

                images.Add(result.Value);
            }
        }

        _schedulerService.ScheduleAuctionStart(auction);

        var auctionDto = new AuctionDto(
            auction.Id,
            auction.Title,
            auction.Description,
            auction.StartDate,
            auction.EndDate,
            auction.StartingPrice,
            null,
            auction.CreatedBy,
            images.Select(x => new AuctionImageDto(x.Id, x.AuctionId, x.ImageUrl, x.IsPrimary)).ToList());

        return auctionDto;
    }
}
