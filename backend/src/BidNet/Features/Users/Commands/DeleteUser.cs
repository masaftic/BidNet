// using BidNet.Data.Persistence;
// using BidNet.Domain.Entities;
// using ErrorOr;
// using FluentValidation;
// using MediatR;
// using Microsoft.EntityFrameworkCore;

// namespace BidNet.Features.Users.Commands;

// public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
// {
//     public DeleteUserCommandValidator()
//     {
//         RuleFor(x => x.UserId)
//             .NotEmpty();
//     }
// }

// public record DeleteUserCommand(UserId UserId) : IRequest<ErrorOr<Deleted>>;

// public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ErrorOr<Deleted>>
// {
//     private readonly AppDbContext _dbContext;

//     public DeleteUserCommandHandler(AppDbContext dbContext)
//     {
//         _dbContext = dbContext;
//     }

//     public async Task<ErrorOr<Deleted>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
//     {
//         var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        
//         if (user is null)
//         {
//             return Error.NotFound(description: "User not found");
//         }

//         // Check if user has any auctions or bids
//         var hasAuctions = await _dbContext.Auctions.AnyAsync(a => a.CreatedBy == user.Id, cancellationToken);
//         if (hasAuctions)
//         {
//             return Error.Conflict(description: "Cannot delete user with existing auctions");
//         }

//         var hasBids = await _dbContext.Bids.AnyAsync(b => b.UserId == user.Id, cancellationToken);
//         if (hasBids)
//         {
//             return Error.Conflict(description: "Cannot delete user with existing bids");
//         }

//         _dbContext.Users.Remove(user);
//         await _dbContext.SaveChangesAsync(cancellationToken);

//         return Result.Deleted;
//     }
// }
