using Bidnet.Domain.Enums;
using Ardalis.GuardClauses;

namespace Bidnet.Domain.Entities;

public readonly record struct UserId(Guid Value)
{
    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid id) => new(id);
}

public class User
{
    public UserId Id { get; set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.Bidder;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private User() { }

    public User(string username, string email, UserRole role)
    {
        Guard.Against.NullOrEmpty(username, nameof(username));
        Guard.Against.NullOrEmpty(email, nameof(email));
        Guard.Against.InvalidInput(email, nameof(email), e => e.Contains('@'), "Invalid email format");

        Username = username;
        Email = email;
        Role = role;
    }

    public User(string username, string email) : this(username, email, UserRole.Bidder)
    {
    }
}
