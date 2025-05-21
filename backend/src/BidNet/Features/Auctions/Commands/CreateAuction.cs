using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Common.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;

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
    }
}

public record CreateAuctionCommand(string Title, string Description, DateTime StartDate, DateTime EndDate, decimal StartingPrice) : IRequest<ErrorOr<Auction>>;

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, ErrorOr<Auction>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _userService;

    public CreateAuctionCommandHandler(AppDbContext dbContext, ICurrentUserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    public async Task<ErrorOr<Auction>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = new Auction(request.Title, request.Description, request.StartDate, request.EndDate, request.StartingPrice, _userService.UserId);

        _dbContext.Auctions.Add(auction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return auction;
    }
}
