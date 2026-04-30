using System.Text.Json.Serialization;

namespace Sabanda.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter), JsonNamingPolicy.CamelCase)]
public enum Frequency
{
    Weekly,
    Fortnightly,
    Monthly
}
