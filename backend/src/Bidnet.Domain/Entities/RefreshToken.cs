using Bidnet.Domain.Entities;

namespace Bidnet.Domain.Entities;

public readonly record struct RefreshTokenId(Guid Value)
{
    public static implicit operator Guid(RefreshTokenId id) => id.Value;
    public static implicit operator RefreshTokenId(Guid id) => new(id);
}

public class RefreshToken
{
    public RefreshTokenId Id { get; init; } = Guid.NewGuid();
    public UserId UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; } = false;

    private RefreshToken() { }

    public RefreshToken(UserId userId, string token, DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
