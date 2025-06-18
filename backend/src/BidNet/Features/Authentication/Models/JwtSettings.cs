using System.ComponentModel.DataAnnotations;

namespace BidNet.Features.Authentication.Models;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    [Required]
    [MinLength(16, ErrorMessage = "Key must be at least 16 characters long.")]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Issuer { get; set; } = string.Empty;
    
    [Required]
    public string Audience { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 1440, ErrorMessage = "Expiration must be between 1 minute and 1440 minutes (24 hours).")]
    public int ExpirationInMinutes { get; set; }
}