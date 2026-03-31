using System.Text.Json.Serialization;

namespace Sabanda.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
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
