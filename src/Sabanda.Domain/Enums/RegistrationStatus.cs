using System.Text.Json.Serialization;

namespace Sabanda.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegistrationStatus
{
    Registered,
    Waitlisted,
    Cancelled
}
