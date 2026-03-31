using Sabanda.Domain.Common;

namespace Sabanda.Domain.Entities;

public class ActiveSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Jti { get; private set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private ActiveSession() { }

    public ActiveSession(Guid userId, string jti, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        UserId = userId;
        Jti = jti;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        RevokedAt = DateTimeOffset.UtcNow;
    }

    public bool IsValid(DateTimeOffset now) => RevokedAt == null && ExpiresAt > now;
}
