using Bidnet.Application.Common.Abstractions;
using Bidnet.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Bidnet.Application.Auctions.Commands;

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

public class UpdateAuctionCommand : IRequest<ErrorOr<Auction>>
{
    public Guid Id { get; }
    public string Title { get; }
    public string Description { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public decimal StartingPrice { get; }

    public UpdateAuctionCommand(Guid id, string title, string description, DateTime startDate, DateTime endDate, decimal startingPrice)
    {
        Id = id;
        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        StartingPrice = startingPrice;
    }
}

public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, ErrorOr<Auction>>
{
    private readonly IAppDbContext _dbContext;

    public UpdateAuctionCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<Auction>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions.FindAsync(request.Id);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found.");
        }

        auction.UpdateDetails(request.Title, request.Description, request.StartDate, request.EndDate, request.StartingPrice);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return auction;
    }
}
