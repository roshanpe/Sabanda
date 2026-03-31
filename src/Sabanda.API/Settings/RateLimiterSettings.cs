namespace Sabanda.API.Settings;

public class RateLimiterSettings
{
    public int LoginPerMinute { get; set; } = 10;
    public int AuthenticatedPerMinute { get; set; } = 300;
}
