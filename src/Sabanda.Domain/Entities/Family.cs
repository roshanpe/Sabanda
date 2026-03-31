using Sabanda.Domain.Common;

namespace Sabanda.Domain.Entities;

public class Family : TenantScopedEntity
{
    public string DisplayName { get; private set; } = string.Empty;
    public Guid PrimaryHolderUserId { get; private set; }
    public string? QrToken { get; private set; }
    public Guid? QrTokenJti { get; private set; }
    public DateTimeOffset? QrTokenIssuedAt { get; private set; }

    private Family() { }

    public Family(Guid tenantId, string displayName, Guid primaryHolderUserId)
    {
        TenantId = tenantId;
        DisplayName = displayName;
        PrimaryHolderUserId = primaryHolderUserId;
    }

    public void SetQrToken(string token, Guid jti)
    {
        QrToken = token;
        QrTokenJti = jti;
        QrTokenIssuedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePrimaryHolder(Guid userId)
    {
        PrimaryHolderUserId = userId;
    }
}
