using Bidnet.Domain.Enums;

namespace Bidnet.Api.Models;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole Role { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RefreshTokenRequest
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class AuthenticationResponse
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
