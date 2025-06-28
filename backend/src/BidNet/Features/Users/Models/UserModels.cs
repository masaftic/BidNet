namespace BidNet.Features.Users.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberConfirmed { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    
    // Wallet information
    public decimal Balance { get; set; }
    public decimal HeldBalance { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Activity stats
    public int TotalAuctions { get; set; }
    public int ActiveAuctions { get; set; }
    public int TotalBids { get; set; }
    public int WonAuctions { get; set; }
}

public class UpdateProfileRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}

public class UserSummaryResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
