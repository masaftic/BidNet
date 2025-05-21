using Bidnet.Application.Common.Abstractions;
using Bidnet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Bidnet.Application.Auctions.Commands;

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
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _userService;

    public CreateAuctionCommandHandler(IAppDbContext dbContext, ICurrentUserService userService)
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
