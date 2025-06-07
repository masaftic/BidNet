using Ardalis.GuardClauses;
using BidNet.Domain.Enums;

namespace BidNet.Domain.Entities;

public readonly record struct UserId(Guid Value)
{
    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid id) => new(id);
}

public class User
{
    public UserId Id { get; init; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public HashSet<UserRole> Roles { get; private set; } = [];
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private User() { }

    public User(string username, string email, string passwordHash, UserRole role)
    {
        Guard.Against.NullOrEmpty(username, nameof(username));
        Guard.Against.NullOrEmpty(email, nameof(email));
        Guard.Against.NullOrEmpty(passwordHash, nameof(passwordHash));
        Guard.Against.InvalidInput(email, nameof(email), e => e.Contains('@'), "Invalid email format");

        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Roles.Add(role);
    }

    public void UpdatePassword(string newPasswordHash)
    {
        Guard.Against.NullOrEmpty(newPasswordHash, nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    public void UpdateProfile(string username, string email)
    {
        Guard.Against.NullOrEmpty(username, nameof(username));
        Guard.Against.NullOrEmpty(email, nameof(email));
        Guard.Against.InvalidInput(email, nameof(email), e => e.Contains('@'), "Invalid email format");

        Username = username;
        Email = email;
    }
}
