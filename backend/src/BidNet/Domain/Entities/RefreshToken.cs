namespace BidNet.Domain.Entities;

public readonly record struct RefreshTokenId(Guid Value)
{
    public static implicit operator Guid(RefreshTokenId id) => id.Value;
    public static implicit operator RefreshTokenId(Guid id) => new(id);

    override public string ToString() => Value.ToString();
}

public class RefreshToken
{
    public RefreshTokenId Id { get; init; }
    public UserId UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;

    private RefreshToken() { }

    public RefreshToken(UserId userId, string token, DateTime expiresAt)
    {
        Id = new RefreshTokenId(Guid.NewGuid());

        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
