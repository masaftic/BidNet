using System.ComponentModel.DataAnnotations;

namespace Bidnet.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    [Required]
    [MinLength(16)]
    public string Key { get; init; } = null!;
    
    [Required]
    [MinLength(16)]
    public string RefreshKey { get; init; } = null!;
    
    [Required]
    public string Issuer { get; init; } = null!;
    
    [Required]
    public string Audience { get; init; } = null!;
}
