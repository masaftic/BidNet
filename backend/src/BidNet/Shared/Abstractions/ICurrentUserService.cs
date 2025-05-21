using BidNet.Domain.Entities;

namespace BidNet.Shared.Abstractions;

public interface ICurrentUserService
{
    UserId UserId { get; }
    string UserName { get; }
}