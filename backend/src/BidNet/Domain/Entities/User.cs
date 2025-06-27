using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;

namespace BidNet.Domain.Entities;

public readonly record struct UserId(Guid Value)
{
    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class User : IdentityUser<UserId>
{
    public User()
    {
        base.Id = Guid.NewGuid();
        base.ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public void UpdateProfile(string userName, string email)
    {
        Guard.Against.NullOrWhiteSpace(userName, nameof(userName));
        Guard.Against.NullOrWhiteSpace(email, nameof(email));

        UserName = userName;
        Email = email;
    }
}
