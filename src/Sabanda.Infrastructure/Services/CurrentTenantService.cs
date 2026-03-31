using Sabanda.Application.Common.Interfaces;

namespace Sabanda.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private Guid _tenantId;
    private string _tenantSlug = string.Empty;
    private bool _isResolved;

    public Guid TenantId => _tenantId;
    public string TenantSlug => _tenantSlug;
    public bool IsResolved => _isResolved;

    public void SetTenant(Guid tenantId, string slug)
    {
        _tenantId = tenantId;
        _tenantSlug = slug;
        _isResolved = true;
    }
}
