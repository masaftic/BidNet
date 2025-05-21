using Bidnet.Application.Common.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Bidnet.Application.Auctions.Commands;

public class DeleteAuctionCommandValidator : AbstractValidator<DeleteAuctionCommand>
{
    public DeleteAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

public record DeleteAuctionCommand(Guid Id) : IRequest<ErrorOr<Unit>>;

public class DeleteAuctionCommandHandler : IRequestHandler<DeleteAuctionCommand, ErrorOr<Unit>>
{
    private readonly IAppDbContext _dbContext;

    public DeleteAuctionCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ErrorOr<Unit>> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _dbContext.Auctions.FindAsync(request.Id);

        if (auction == null)
        {
            return Error.NotFound(description: "Auction not found.");
        }

        _dbContext.Auctions.Remove(auction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
