using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class AppUser : TenantScopedEntity
{
    public Guid? FamilyId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }

    private AppUser() { }

    public AppUser(Guid tenantId, string email, string passwordHash, UserRole role, Guid? familyId = null)
    {
        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        FamilyId = familyId;
        FailedLoginCount = 0;
    }

    public void RecordFailedLogin(DateTimeOffset now)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= 5)
            LockedUntil = now.AddMinutes(15);
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
    }

    public bool IsLockedOut(DateTimeOffset now) => LockedUntil.HasValue && LockedUntil.Value > now;

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void AssignFamily(Guid familyId)
    {
        FamilyId = familyId;
    }
}
