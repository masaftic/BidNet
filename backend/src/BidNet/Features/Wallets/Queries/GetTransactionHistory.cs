using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Features.Wallets.Mapping;
using BidNet.Features.Wallets.Models;
using BidNet.Shared.Services;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Wallets.Queries;

public record GetTransactionHistoryQuery(int Page = 1, int PageSize = 20) : IRequest<ErrorOr<TransactionHistoryResponse>>;

public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, ErrorOr<TransactionHistoryResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionHistoryQueryHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<TransactionHistoryResponse>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        var wallet = await _dbContext.Wallets
            .AsNoTracking()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            // Return an empty transaction history if no wallet exists
            return new TransactionHistoryResponse
            {
                UserId = userId,
                Transactions = new List<TransactionDto>(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        var transactions = wallet.Transactions
            .OrderByDescending(t => t.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select<WalletTransaction, TransactionDto>(t => t.ToTransactionDto())
            .ToList();

        return new TransactionHistoryResponse
        {
            UserId = userId,
            Transactions = transactions,
            TotalCount = wallet.Transactions.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
