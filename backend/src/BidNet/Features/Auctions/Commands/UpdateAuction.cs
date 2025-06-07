using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Auctions.Commands;

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0);
    }
}

public record UpdateAuctionCommand(Guid Id, string Title, string Description, DateTime StartDate, DateTime EndDate, decimal StartingPrice) : IRequest<ErrorOr<Auction>>;

public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, ErrorOr<Auction>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _userService;

    public UpdateAuctionCommandHandler(AppDbContext dbContext, ICurrentUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<ErrorOr<Auction>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions.FindAsync(request.Id);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found.");
        }

        if (auction.CreatedBy != _userService.UserId || !_userService.IsInRole("Admin"))
        {
            return Error.Forbidden(description: "You do not have permission to update this auction.");
        }

        auction.UpdateDetails(request.Title, request.Description, request.StartDate, request.EndDate, request.StartingPrice);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return auction;
    }
}
