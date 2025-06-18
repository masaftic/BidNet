using Ardalis.GuardClauses;
using BidNet.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace BidNet.Domain.Entities;

public readonly record struct UserId(Guid Value)
{
    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid id) => new(id);
}

public class User : IdentityUser<UserId>
{
    public User()
    {
        base.Id = Guid.NewGuid();
        base.ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}
