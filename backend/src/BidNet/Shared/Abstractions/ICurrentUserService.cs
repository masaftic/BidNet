using BidNet.Domain.Entities;

namespace BidNet.Features.Common.Abstractions;

public interface ICurrentUserService
{
    UserId UserId { get; }
    string UserName { get; }
}