namespace Sabanda.Domain.Enums;

public enum AuditAction
{
    Login,
    LoginFailed,
    Logout,
    MinorDataRead,
    RoleChanged,
    QrTokenRegenerated,
    FamilyCreated,
    MemberCreated
}
