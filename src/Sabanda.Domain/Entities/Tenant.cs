using Sabanda.Domain.Common;

namespace Sabanda.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? SettingsJson { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public Tenant(string name, string slug)
    {
        Name = name;
        Slug = slug.ToLowerInvariant();
    }
}
