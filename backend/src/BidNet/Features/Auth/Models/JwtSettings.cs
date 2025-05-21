using System.ComponentModel.DataAnnotations;

namespace BidNet.Features.Auth.Models;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(16)]
    public string Key { get; init; } = null!;

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;

    [Required]
    public int ExpirationInMinutes { get; init; }
}
