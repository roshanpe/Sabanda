namespace Sabanda.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid TenantId { get; }
    string TenantSlug { get; }
    bool IsResolved { get; }
    void SetTenant(Guid tenantId, string slug);
}
