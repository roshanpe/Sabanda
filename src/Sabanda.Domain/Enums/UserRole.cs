using System.Text.Json.Serialization;

namespace Sabanda.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Administrator,
    PrimaryAccountHolder,
    FamilyMember,
    ProgramCoordinator,
    EventCoordinator
}
