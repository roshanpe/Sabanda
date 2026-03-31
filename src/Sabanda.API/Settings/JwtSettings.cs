using System.ComponentModel.DataAnnotations;

namespace Sabanda.API.Settings;

public class JwtSettings
{
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryHours { get; set; } = 8;

    public string Issuer { get; set; } = "sabanda";
    public string Audience { get; set; } = "sabanda";
}
