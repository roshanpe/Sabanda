using System.ComponentModel.DataAnnotations;

namespace Sabanda.API.Settings;

public class QrTokenSettings
{
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryDays { get; set; } = 90;
}
